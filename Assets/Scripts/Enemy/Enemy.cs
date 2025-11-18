using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : LivingEntity, ITargetable
{
    private ObjectPoolManager<int, Enemy> objectPoolManager;

    private EnemyMovement movement;
    public EnemyMovement Movement => movement;
    private EnemyPattern pattern;
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
    private CancellationTokenSource lifeTimeCts;

    [SerializeField] private List<DropItem> drops;

    //test
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private Color hitColor = Color.white;
    private Material Material;

    private CancellationTokenSource colorResetCts;
    private int enemyId;
    private int patternId = 1; //test
    public EnemySpawner Spawner { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();

        OnDeathEvent += SpawnManager.Instance.OnEnemyDied;

        Material = GetComponent<Renderer>().material;
        Material.color = baseColor;
        ColorCancel();

        originalScale = transform.localScale;
    }

    protected void OnDestroy()
    {
        OnDeathEvent -= SpawnManager.Instance.OnEnemyDied;
        Cancel();

        ColorCancel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            return;
        }
        
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.OnDamage(data.Attack);
        }

        pattern?.OnTrigger(other);
    }

    public override void OnDamage(float damage)
    {
        float actualDamage = pattern != null ? pattern.CalculateDamage(damage) : damage;

        base.OnDamage(actualDamage);

        ColorCancel();

        Material.color = hitColor;
        ResetColorAsync(0.2f, colorResetCts.Token).Forget();
    }

    protected override void Die()
    {
        base.Die();

        Cancel();

        foreach (var drop in drops)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
        }

        transform.localScale = originalScale;

        objectPoolManager?.Return(enemyId, this);
    }

    public void Initialize(EnemyTableData enemyData, Vector3 targetDirection, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, bool excutePattern)
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

        AddMovementComponent();

        AddPatternComponent(data.EnemyGrade, patternId);

        Cancel();

        LifeTimeTask(lifeTimeCts.Token).Forget();

        if(excutePattern && pattern != null)
        {
            pattern.Initialize(this, movement, data);
        }
    }

    private void Cancel()
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
                objectPoolManager?.Return(enemyId, this);
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
            Cancel();
        }
    }

    public void ResumeLifeTime()
    {
        if(isLifeTimePaused)
        {
            isLifeTimePaused = false;
            Cancel();
            LifeTimeTask(lifeTimeCts.Token).Forget();
        }
    }

    private void AddMovementComponent()
    {
        if(movement == null)
        {
            movement = gameObject.AddComponent<StraightDownMovement>();
        }

        movement.Initialize(moveSpeed, Vector3.down);
        //movement.Initialize(1f, Vector3.down);
    }

    private void AddPatternComponent(int grade, int patternId)
    {
        if(pattern != null)
        {
            Destroy(pattern);
        }

        //Grade 4: Normal, Gade 3: Unique, Grade: Middle Boss, Grade 1: Boss
        if(grade == 4)
        {
            pattern = gameObject.AddComponent<NormalPattern>();
        }
        else
        {
            switch (patternId)
            {
                case 0:
                    pattern = gameObject.AddComponent<MeteorClusterPattern>();
                    break;
                default:
                    pattern = gameObject.AddComponent<NormalPattern>();
                    break;
            }
        }
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
}
