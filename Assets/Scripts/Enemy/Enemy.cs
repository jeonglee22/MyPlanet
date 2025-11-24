using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : LivingEntity, ITargetable , IDisposable
{
    private ObjectPoolManager<int, Enemy> objectPoolManager;
    public ObjectPoolManager<int, Enemy> ObjectPoolManager => objectPoolManager;

    private EnemyMovement movement;
    public EnemyMovement Movement => movement;
    private PatternExecutor patternExecutor;
    private EnemyTableData data;
    public EnemyTableData Data { get { return data; } }

    public Vector3 position => transform.position;

    public bool isAlive => !IsDead;

    public float maxHp => maxHealth;

    public float atk => attack;

    public float def => defense;
    private float attack;
    private float defense;
    private float moveSpeed;
    private float ratePenetration;
    private float fixedPenetration;
    private Vector3 originalScale;
    [SerializeField] private float lifeTime = 2f;
    public float LifeTime => lifeTime;
    public float RemainingLifeTime => remainingLifeTime > 0 ? remainingLifeTime : lifeTime;
    private CancellationTokenSource lifeTimeCts;

    [SerializeField] private List<DropItem> drops;

    //test
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private Color hitColor = Color.white;
    private Material Material;

    private CancellationTokenSource colorResetCts;
    private int enemyId;
    public EnemySpawner Spawner { get; set; }

    public event Action OnLifeTimeOverEvent;

    public Func<float> OnCollisionDamageCalculate { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();
        patternExecutor = GetComponent<PatternExecutor>();

        OnDeathEvent += SpawnManager.Instance.OnEnemyDied;
        OnLifeTimeOverEvent += SpawnManager.Instance.OnEnemyDied;

        Material = GetComponent<Renderer>().material;
        Material.color = baseColor;
        ColorCancel();

        originalScale = transform.localScale;

        OnCollisionDamageCalculate = null;

        remainingLifeTime = 0f;
        isLifeTimePaused = false;
    }

    protected virtual void OnDisable() 
    {
        OnDeathEvent -= SpawnManager.Instance.OnEnemyDied;
        OnLifeTimeOverEvent -= SpawnManager.Instance.OnEnemyDied;

        if(patternExecutor != null)
        {
            patternExecutor.ClearPatterns();
        }
        
        movement = null;

        OnCollisionDamageCalculate = null;

        StopLifeTime();
    }

    protected void OnDestroy()
    {
        OnDeathEvent -= SpawnManager.Instance.OnEnemyDied;
        OnLifeTimeOverEvent -= SpawnManager.Instance.OnEnemyDied;

        StopLifeTime();
        ColorCancel();
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);

        ColorCancel();

        Material.color = hitColor;
        ResetColorAsync(0.2f, colorResetCts.Token).Forget();
    }

    public override void Die()
    {
        base.Die();

        StopLifeTime();

        foreach (var drop in drops)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
        }

        transform.localScale = originalScale;

        objectPoolManager?.Return(enemyId, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagName.Enemy))
        {
            return;
        }
        
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            float damage = OnCollisionDamageCalculate?.Invoke() ?? attack;
            damagable.OnDamage(attack);

            if(IsDead)
            {
                return;
            }
            
            IsDead = true;
            StopLifeTime();
            OnLifeTimeOverEvent?.Invoke();
            ReturnToPool();
            return;
        }

        if(other.CompareTag("PatternLine"))
        {
            OnPatternLineTrigger();
        }
    }

    private void ReturnToPool()
    {
        if(objectPoolManager != null && objectPoolManager.HasPool(enemyId))
        {
            objectPoolManager.Return(enemyId, this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnLifeTimeOver()
    {
        OnLifeTimeOverEvent?.Invoke();
        transform.localScale = originalScale;
        objectPoolManager?.Return(enemyId, this);
    }

    public void Initialize(EnemyTableData enemyData, Vector3 targetDirection, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int spawnPointIndex)
    {
        this.enemyId = enemyId;
        objectPoolManager = poolManager;

        data = enemyData;
        maxHealth = data.Hp * scaleData.HpScale;
        Health = maxHealth;

        attack = data.Attack * scaleData.AttScale;
        defense = data.Defense * scaleData.DefScale;
        moveSpeed = data.MoveSpeed * scaleData.MoveSpeedScale;

        ratePenetration = data.UniqueRatePenetration * scaleData.PenetScale;
        fixedPenetration = data.FixedPenetration * scaleData.PenetScale;

        transform.localScale *= scaleData.PrefabScale;

        AddMovementComponent(data.MoveType, spawnPointIndex);

        InitializePatterns(enemyData);

        StartLifeTime();
    }

    public void InitializeAsChild(EnemyTableData enemyData, Vector3 targetDirection, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, IMovement movementComponent)
    {
        this.enemyId = enemyId;
        objectPoolManager = poolManager;

        data = enemyData;
        maxHealth = enemyData.Hp * scaleData.HpScale;
        Health = maxHealth;

        attack = enemyData.Attack * scaleData.AttScale;
        defense = enemyData.Defense * scaleData.DefScale;
        moveSpeed = enemyData.MoveSpeed * scaleData.MoveSpeedScale;

        ratePenetration = enemyData.UniqueRatePenetration * scaleData.PenetScale;
        fixedPenetration = enemyData.FixedPenetration * scaleData.PenetScale;

        transform.localScale *= scaleData.PrefabScale;

        if(movement == null)
        {
            movement = gameObject.AddComponent<EnemyMovement>();
        }
        movement.Initialize(moveSpeed, -1, movementComponent);

        if(patternExecutor == null)
        {
            patternExecutor = gameObject.AddComponent<PatternExecutor>();
        }
        patternExecutor.Initialize(this);

        StartLifeTime();
    }

    private void StartLifeTime()
    {
        StopLifeTime();

        lifeTimeCts = new CancellationTokenSource();
        LifeTimeTask(lifeTimeCts.Token).Forget();
    }

    public void StopLifeTime()
    {
        lifeTimeCts?.Cancel();
        lifeTimeCts?.Dispose();
        lifeTimeCts = new CancellationTokenSource();
    }

    private async UniTaskVoid LifeTimeTask(CancellationToken token)
    {
        float timeToWait = 0f;
        float startTime = 0f;

        try
        {
            timeToWait = remainingLifeTime > 0f ? remainingLifeTime : lifeTime;
            startTime = Time.time;

            await UniTask.Delay(System.TimeSpan.FromSeconds(timeToWait), cancellationToken: token);
            if(!token.IsCancellationRequested)
            {
                OnLifeTimeOver();
            }
        }
        catch (System.OperationCanceledException)
        {
            if (isLifeTimePaused)
            {
                float elapsed = Time.time - startTime;
                remainingLifeTime = timeToWait - elapsed;
            }
        }
    }

    private float remainingLifeTime;
    private bool isLifeTimePaused;
    public void PauseLifeTime()
    {
        if(!isLifeTimePaused)
        {
            isLifeTimePaused = true;
            StopLifeTime();
        }
    }

    public void ResumeLifeTime()
    {
        if(isLifeTimePaused)
        {
            isLifeTimePaused = false;
            StartLifeTime();
            LifeTimeTask(lifeTimeCts.Token).Forget();
        }
    }

    private void AddMovementComponent(int moveType, int spawnPointIndex)
    {
        if(movement == null)
        {
            movement = gameObject.AddComponent<EnemyMovement>();
        }

        IMovement movementComponent = MovementManager.Instance.GetMovement(moveType);

        movement.Initialize(moveSpeed, spawnPointIndex, movementComponent);
    }

    private void InitializePatterns(EnemyTableData enemyData)
    {
        if(patternExecutor == null)
        {
            patternExecutor = gameObject.AddComponent<PatternExecutor>();
        }

        patternExecutor.Initialize(this);

        List<int> patternIds = new List<int>{0}; //test

        foreach(var patternId in patternIds)
        {
            IPattern pattern = PatternManager.Instance.GetPattern(patternId);
            if(pattern != null)
            {
                pattern.Initialize(this, movement, enemyData);
                patternExecutor.AddPattern(pattern);
            }
        }
    }

    public void OnPatternLineTrigger()
    {
        movement?.OnPatternLine();
        patternExecutor?.OnPatternLine();
    }

    //test
    private void ColorCancel()
    {
        colorResetCts?.Cancel();
        colorResetCts?.Dispose();
        colorResetCts = new CancellationTokenSource();
    }

    private async UniTaskVoid ResetColorAsync(float delay, CancellationToken token = default)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: colorResetCts.Token);
        Material.color = baseColor;
    }

    public void Dispose()
    {
        
    }
}
