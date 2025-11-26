using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class Enemy : LivingEntity, ITargetable , IDisposable
{
    private ObjectPoolManager<int, Enemy> objectPoolManager;

    private EnemyMovement movement;
    public EnemyMovement Movement { get => movement; set => movement = value; }
    private PatternExecutor patternExecutor;
    private EnemyTableData data;
    public EnemyTableData Data { get { return data; } }
    public PatternData CurrentPatternData { get; private set; }

    public ScaleData ScaleData { get; private set; }

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
    private float lifeTime = 15f;
    public float LifeTime => lifeTime;
    private CancellationTokenSource lifeTimeCts;
    private float exp;

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

    public bool ShouldDropItems { get; set; } = true;

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();
        patternExecutor = GetComponent<PatternExecutor>();

        movement?.Initialize(moveSpeed, -1, movement.CurrentMovement);
        patternExecutor?.Initialize(this);

        OnDeathEvent += SpawnManager.Instance.OnEnemyDied;
        OnLifeTimeOverEvent += SpawnManager.Instance.OnEnemyDied;

        Material = GetComponent<Renderer>().material;
        Material.color = baseColor;
        ColorCancel();

        OnCollisionDamageCalculate = null;
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

        if (ShouldDropItems)
        {
            foreach (var drop in drops)
            {
                var dropInstance = Instantiate(drop, transform.position, Quaternion.identity);
                if(dropInstance is ExpItem expItem)
                {
                    expItem.SetExp(exp);
                }
            }
        }

        transform.localScale = originalScale;

        objectPoolManager?.Return(enemyId, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PatternLine"))
        {
            OnPatternLineTrigger();
        }

        if (other.gameObject.CompareTag(TagName.Enemy))
        {
            return;
        }
        
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            float damage = OnCollisionDamageCalculate?.Invoke() ?? attack;
            damagable.OnDamage(damage);

            if(IsDead)
            {
                return;
            }
            
            IsDead = true;
            StopLifeTime();
            OnLifeTimeOverEvent?.Invoke();
            transform.localScale = originalScale;
            ReturnToPool();
            return;
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
        ScaleData = scaleData;
        maxHealth = data.Hp * scaleData.HpScale;
        Health = maxHealth;

        attack = data.Attack * scaleData.AttScale;
        defense = data.Defense * scaleData.DefScale;
        moveSpeed = data.MoveSpeed * scaleData.MoveSpeedScale;

        ratePenetration = data.UniqueRatePenetration * scaleData.PenetScale;
        fixedPenetration = data.FixedPenetration * scaleData.PenetScale;

        originalScale = transform.localScale;

        transform.localScale *= scaleData.PrefabScale;

        exp = data.Exp * scaleData.ExpScale;

        AddMovementComponent(data.MoveType, spawnPointIndex);

        InitializePatterns(enemyData);

        StartLifeTime();
    }

    public void InitializeAsChild(EnemyTableData enemyData, Vector3 targetDirection, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int moveType)
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

        originalScale = transform.localScale;

        transform.localScale *= scaleData.PrefabScale;

        AddMovementComponent(moveType, -1);

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
        try
        {
            if(data.EnemyGrade <= 2)
            {
                return;
            }
            await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTime), cancellationToken: token);
            
            if(!token.IsCancellationRequested)
            {
                OnLifeTimeOver();
            }
        }
        catch (System.OperationCanceledException)
        {
            
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

        if(enemyData.PatternList == 0)
        {
            return;
        }
        List<PatternData> patterns = DataTableManager.PatternTable.GetPatternList(enemyData.PatternList);

        foreach(var patternItem in patterns)
        {
            CurrentPatternData = patternItem;

            IPattern pattern = PatternManager.Instance.GetPattern(patternItem.Pattern_Id);
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
