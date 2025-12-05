using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
    private float lifeTime = 45f;
    public float LifeTime => lifeTime;
    private CancellationTokenSource lifeTimeCts;
    private float exp;

    private int enemyType;
    public int EnemyType => enemyType;

    [SerializeField] private List<DropItem> drops;

    private CancellationTokenSource colorResetCts;
    private int enemyId;
    public EnemySpawner Spawner { get; set; }

    public event Action OnLifeTimeOverEvent;

    public Func<float> OnCollisionDamageCalculate { get; set; }

    public bool ShouldDropItems { get; set; } = true;

    private bool isTutorial = false;

    private void Start()
    {
        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();
        patternExecutor = GetComponent<PatternExecutor>();

        movement?.Initialize(moveSpeed, -1, movement.CurrentMovement);
        patternExecutor?.Initialize(this);

        OnDeathEvent += SpawnManager.Instance.OnEnemyDied;
        OnLifeTimeOverEvent += SpawnManager.Instance.OnEnemyDied;

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
    }

    public override void OnDamage(float damage)
    {
        if(data.EnemyType == 4 && Variables.MiddleBossEnemy != null)
        {
            return;
        }

        base.OnDamage(damage);
    }

    public override void Die()
    {
        base.Die();

        BossDie(data.EnemyType);

        StopLifeTime();

        if (ShouldDropItems)
        {
            foreach (var drop in drops)
            {
                if(drop is QuasarItem && data.EnemyGrade != 2)
                {
                    continue;
                }

                if(drop is QuasarItem)
                {
                    UnityEngine.Debug.Log("Drop Quasar");
                }

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

    public void BossDie(int enemyType)
    {
        switch (enemyType)
        {
            case 3:
                Variables.MiddleBossEnemy = null;
                WaveManager.Instance.OnBossDefeated(false);
                if(isTutorial && Variables.Stage == 2)
                {
                    TutorialManager.Instance.ShowTutorialStep(10);
                }
                break;
            case 4:
                Variables.LastBossEnemy = null;
                WaveManager.Instance.OnBossDefeated(true);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PatternLine"))
        {
            OnPatternLineTrigger();
        }

        if(other.CompareTag(TagName.CenterStone) || data.EnemyType == 4 || data.EnemyType == 3)
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

    public void OnLifeTimeOver()
    {
        OnLifeTimeOverEvent?.Invoke();
        transform.localScale = originalScale;
        objectPoolManager?.Return(enemyId, this);
    }

    public void Initialize(EnemyTableData enemyData, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int spawnPointIndex)
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

        enemyType = data.EnemyType;

        BossAppearance(enemyData.EnemyType);

        AddMovementComponent(data.MoveType, spawnPointIndex);

        InitializePatterns(enemyData);

        StartLifeTime();
    }

    public void InitializeAsChild(EnemyTableData enemyData, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int moveType)
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

        exp = data.Exp * scaleData.ExpScale;

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

    private void BossAppearance(int enemyType)
    {
        var radius = gameObject.GetComponent<SphereCollider>().radius * transform.localScale.x;

        switch (enemyType)
        {
            case 3:
                Variables.MiddleBossEnemy = this;
                WaveManager.Instance.OnBossSpawned(false);
                transform.position = Spawner.transform.position + new Vector3(0f, radius, 0f);

                if(isTutorial && Variables.Stage == 2)
                {
                    TutorialManager.Instance.ShowTutorialStep(9);
                }
                break;
            case 4:
                Variables.LastBossEnemy = this;
                WaveManager.Instance.OnBossSpawned(true);
                transform.position = Spawner.transform.position + new Vector3(0f, radius, 0f);

                if(isTutorial && Variables.Stage == 2)
                {
                    TutorialManager.Instance.ShowTutorialStep(11);
                }
                break;
        }

        if(enemyType == 3 || enemyType == 4)
        {
            SpawnManager.Instance.DespawnAllEnemiesExceptBoss();
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

    public void Dispose()
    {
        
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }
}