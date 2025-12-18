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
    public float MoveSpeed => moveSpeed;
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

    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;
    private bool hasReachedStartPosition = false;

    private bool isTargetable = true;
    public bool IsTargetable => isTargetable;

    public Enemy ParentEnemy { get; set; }

    private Planet planet;
    
    public List<Enemy> ChildEnemy { get; set; } = new List<Enemy>();
    public bool IsReflectShieldActive { get; set; } = false;

    public GameObject ReflectShieldObject { get; set; }

    public bool HasHit { get; set; } = false;

    [Header("Boss SFX")]
    [SerializeField] private AudioSource bossAudioSource;
    [SerializeField] private AudioClip middleBossSpawnSfx;
    [SerializeField] private AudioClip lastBossSpawnSfx;
    [SerializeField, Range(0f, 1f)] private float bossSpawnSfxVolume = 1f;
    [SerializeField] private float bossSpawnSfxGapSeconds = 0.15f;

    [Header("Pattern SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip simpleShotSfx;
    [SerializeField, Range(0f, 1f)] private float simpleShotVolume = 1f;
    private void Start()
    {
        SetIsTutorial(TutorialManager.Instance?.IsTutorialMode ?? false);

        planet = GameObject.FindWithTag(TagName.Planet).GetComponent<Planet>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();
        patternExecutor = GetComponent<PatternExecutor>();

        movement?.Initialize(moveSpeed, -1, enemyType, movement.CurrentMovement);
        patternExecutor?.Initialize(this);

        if (!Variables.IsTestMode)
        {
            OnDeathEvent += SpawnManager.Instance.OnEnemyDied;
            OnLifeTimeOverEvent += SpawnManager.Instance.OnEnemyDied;

            OnCollisionDamageCalculate = null;
        }
    }

    protected virtual void OnDisable() 
    {
        if (!Variables.IsTestMode)
        {
            OnDeathEvent -= SpawnManager.Instance.OnEnemyDied;
            OnLifeTimeOverEvent -= SpawnManager.Instance.OnEnemyDied;
        }

        if(patternExecutor != null)
        {
            patternExecutor.ClearPatterns();
        }
        
        movement = null;

        OnCollisionDamageCalculate = null;

        ParentEnemy = null;

        ChildEnemy.Clear();

        StopLifeTime();

        HasHit = false;
    }

    protected void OnDestroy()
    {
        OnDeathEvent -= SpawnManager.Instance.OnEnemyDied;
        OnLifeTimeOverEvent -= SpawnManager.Instance.OnEnemyDied;

        StopLifeTime();
    }

    private void Update()
    {
        if((enemyType == 3 || enemyType == 4) && !hasReachedStartPosition)
        {
            if(movement?.CurrentMovement != null && movement.CurrentMovement.IsCompleted())
            {
                OnReachedStartPosition();
            }
        }
    }

    private void OnReachedStartPosition()
    {
        hasReachedStartPosition = true;
        isInvincible = false;
        isTargetable=true;

        if(patternExecutor != null)
        {
            patternExecutor.OnBossReady();
        }
    }

    public override void OnDamage(float damage)
    {
        if(isInvincible)
        {
            return;
        }

        if(data.EnemyType == 4 && Variables.MiddleBossEnemy != null)
        {
            return;
        }

        DpsCalculator.AddDamage(damage);

        planet.Health += damage * planet.PlanetData.Drain;

        base.OnDamage(damage);

        HasHit = true;
    }

    private void LateUpdate()
    {
        HasHit = false;
    }

    public override void Die()
    {
        base.Die();

        BossDie(data.EnemyType);

        StopLifeTime();

        if (ShouldDropItems && Variables.IsTestMode == false)
        {
            foreach (var drop in drops)
            {
                if(drop is QuasarItem && (enemyType != 3 || Variables.LastBossEnemy != null))
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
                    TutorialManager.Instance?.ShowTutorialStep(10);
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

        if(other.CompareTag(TagName.Planet))
        {
            Debug.Log("Planet");
        }

        if(other.CompareTag(TagName.CenterStone) || data.EnemyType == 4 || data.EnemyType == 3)
        {
            return;
        }
        
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            float damage = OnCollisionDamageCalculate?.Invoke() ?? attack;
            var planet = damagable as Planet;
            if (planet != null)
            {
                if (planet.Shield > 0)
                {
                    if (planet.Shield >= damage)
                    {
                        planet.Shield -= damage;
                        damage = 0;
                    }
                    else
                    {
                        damage -= planet.Shield;
                        planet.Shield = 0;
                    }
                }
                damagable.OnDamage(CalculateTotalDamage(planet.Defense, damage));
            }

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

    public float CalculateTotalDamage(float planetDef, float damage)
    {
        if (damage < 0f)
        {
            damage = 0f;
        }

        var RatePanetration = Mathf.Clamp(ratePenetration, 0f, 100f);
        // Debug.Log(damage);
        var totalPlanetDef = planetDef * (1 - RatePanetration / 100f) - fixedPenetration;
        if(totalPlanetDef < 0)
        {
            totalPlanetDef = 0;
        }
        var totalDamage = damage * 100f / (100f + totalPlanetDef);
        
        return totalDamage;
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
        enemyType = data.EnemyType;
        ScaleData = scaleData;
        maxHealth = data.Hp * scaleData.HpScale;
        Health = maxHealth;

        Debug.Log($"Initializing Enemy ID: {enemyId}, Type: {enemyType}, HP: {maxHealth}");

        attack = data.Attack * scaleData.AttScale;
        defense = data.Defense * scaleData.DefScale;
        moveSpeed = data.MoveSpeed * scaleData.MoveSpeedScale;

        ratePenetration = data.UniqueRatePenetration * scaleData.PenetScale;
        fixedPenetration = data.FixedPenetration * scaleData.PenetScale;

        originalScale = transform.localScale;

        transform.localScale *= scaleData.PrefabScale;

        exp = data.Exp * scaleData.ExpScale;

        enemyType = data.EnemyType;
        isTargetable = true;

        ReflectShieldObject = GetComponentInChildren<ReflectShield>(true)?.gameObject;

        BossAppearance(enemyData.EnemyType);

        AddMovementComponent(data.MoveType, spawnPointIndex);

        InitializePatterns(enemyData);

        StartLifeTime();
    }

    public void InitializeAsChild(EnemyTableData enemyData, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int moveType, Enemy parent)
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
        
        isTargetable = true;

        ParentEnemy = parent;

        ReflectShieldObject = GetComponentInChildren<ReflectShield>(true)?.gameObject;

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
        if(enemyType != 3 && enemyType != 4)
        {
            return;
        }

        PlayBossSpawnSfx(enemyType);

        var radius = gameObject.GetComponent<SphereCollider>().radius * transform.localScale.x;
        transform.position = Spawner.transform.position + new Vector3(0f, radius, 0f);

        isInvincible = true;
        hasReachedStartPosition = false;
        isTargetable = false;

        switch (enemyType)
        {
            case 3:
                Variables.MiddleBossEnemy = this;
                WaveManager.Instance.OnBossSpawned(false);

                if(isTutorial && Variables.Stage == 2)
                {
                    TutorialManager.Instance?.ShowTutorialStep(9);
                }
                break;
            case 4:
                Variables.LastBossEnemy = this;
                WaveManager.Instance.OnBossSpawned(true);

                if(isTutorial && Variables.Stage == 2)
                {
                    TutorialManager.Instance?.ShowTutorialStep(11);
                }
                break;
        }

        SpawnManager.Instance.DespawnAllEnemiesExceptBoss();
    }

    private void AddMovementComponent(int moveType, int spawnPointIndex)
    {
        if(movement == null)
        {
            movement = gameObject.AddComponent<EnemyMovement>();
        }

        IMovement movementComponent = MovementManager.Instance.GetMovement(moveType);

        movement.Initialize(moveSpeed, spawnPointIndex, enemyType, movementComponent);
    }

    private void InitializePatterns(EnemyTableData enemyData)
    {
        if(patternExecutor == null)
        {
            patternExecutor = gameObject.AddComponent<PatternExecutor>();
        }

        patternExecutor.Initialize(this);

        if(enemyData.PatternGroup == 0)
        {
            return;
        }
        List<PatternData> patterns = DataTableManager.PatternTable.GetPatternList(enemyData.PatternGroup);

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

    private void EnsureBossAudioSource()
    {
        if (bossAudioSource != null) return;

        bossAudioSource = GetComponent<AudioSource>();
        if (bossAudioSource == null) bossAudioSource = gameObject.AddComponent<AudioSource>();

        bossAudioSource.playOnAwake = false;
        bossAudioSource.loop = false;
        bossAudioSource.spatialBlend = 0f;
    }

    private void PlayBossSpawnSfx(int enemyType)
    {
        AudioClip clip = null;
        if (enemyType == 3) clip = middleBossSpawnSfx;
        else if (enemyType == 4) clip = lastBossSpawnSfx;

        if (clip == null) return;

        EnsureBossAudioSource();
        PlayBossSpawnSfxRepeatAsync(clip, 3).Forget();
    }
    private async UniTaskVoid PlayBossSpawnSfxRepeatAsync(AudioClip clip, int count)
    {
        for (int i = 0; i < count; i++)
        {
            bossAudioSource.PlayOneShot(clip, bossSpawnSfxVolume);
            if (i < count - 1)
            {
                var wait = clip.length + bossSpawnSfxGapSeconds;
                await UniTask.Delay(TimeSpan.FromSeconds(wait), ignoreTimeScale: true);
            }
        }
    }

    private void EnsureSfxAudioSource()
    {
        if (sfxAudioSource != null) return;

        sfxAudioSource = GetComponent<AudioSource>();
        if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();

        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.loop = false;
        sfxAudioSource.spatialBlend = 0f;
    }
    public void PlaySimpleShotSfx()
    {
        if (simpleShotSfx == null) return;

        EnsureSfxAudioSource();
        sfxAudioSource.PlayOneShot(simpleShotSfx, simpleShotVolume);
    }
}