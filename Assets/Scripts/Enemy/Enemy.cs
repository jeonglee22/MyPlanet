using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : LivingEntity, ITargetable
{
    private IObjectPool<Enemy> pool;

    private EnemyMovement movement;
    private List<EnemyAbility> abilities = new List<EnemyAbility>();
    private EnemyData data;
    public EnemyData Data { get { return data; } }

    public Vector3 position => transform.position;

    public bool isAlive => !IsDead;

    public float maxHp => maxHealth;

    public float atk => data.damage;

    public float def => data.defense;

    [SerializeField] private float lifeTime = 2f;
    private CancellationTokenSource lifeTimeCts;

    [SerializeField] private List<DropItem> drops;

    //test
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private Color hitColor = Color.white;
    private Material Material;

    private CancellationTokenSource colorResetCts;

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();

        OnDeathEvent += SpawnManager.Instance.OnEnemyDied;

        Material = GetComponent<Renderer>().material;
        Material.color = baseColor;
        ColorCancel();
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
            damagable.OnDamage(data.damage);
        }
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);

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

        pool?.Release(this);
    }

    public void Initialize(EnemyData enemyData, Vector3 targetDirection)
    {
        //tower system random dummy test
        data = ScriptableObject.Instantiate(enemyData);

        //data = enemyData;
        maxHealth = data.maxHealth;
        Health = maxHealth;

        AddMovementComponent(data.movementType, data.speed, targetDirection);
        AddAbilityComponents(data.abilityTypes);

        Cancel();

        LifeTimeTask(lifeTimeCts.Token).Forget();
    }

    public void SetPool(IObjectPool<Enemy> pool)
    {
        this.pool = pool;
    }

    private void Cancel()
    {
        lifeTimeCts?.Cancel();
        lifeTimeCts?.Dispose();
        lifeTimeCts = new CancellationTokenSource();
    }

    private async UniTaskVoid LifeTimeTask(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTime), cancellationToken: token);
            if(!token.IsCancellationRequested)
            {
                pool?.Release(this);
            }
        }
        catch (System.OperationCanceledException)
        {
            // Ignore cancellation
        }
    }

    private void AddMovementComponent(MovementType type, float speed, Vector3 targetDirection)
    {
        if(movement == null)
        {
            switch (type)
            {
                case MovementType.StraightDown:
                    movement = gameObject.AddComponent<StraightDownMovement>();
                    break;
                case MovementType.TargetDirection:
                    movement = gameObject.AddComponent<TargetDirectionMovement>();
                    break;
            }
        }

        movement.Initialize(speed, targetDirection);
    }
    
    private void AddAbilityComponents(AbilityType[] types)
    {
        if (types == null || types.Length == 0)
        {
            return;
        }
        
        foreach(var type in types)
        {
            EnemyAbility ability = null;
            switch (type)
            {
                case AbilityType.SplitOnDeath:
                    ability = gameObject.AddComponent<SplitDeathAbility>();
                    break;
            }

            if(ability != null)
            {
                ability.Initialize(this, data);
                abilities.Add(ability);
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
