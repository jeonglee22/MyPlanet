using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour , IDisposable
{
    [SerializeField] private TrailRenderer trailRenderer;
    public ProjectileData projectileData; //Buffed Data
    private ProjectileData poolKeyData; //BaseDataSO for Key
    public ProjectileData BaseData => poolKeyData;

    public Vector3 direction;
    public bool isHit;

    private Planet planet;

    private Transform currentTarget; //Type: Homing

    //Projectile Data
    public float damage = 10f;
    public float RatePanetration { get;  set; }
    public float FixedPanetration { get;  set; }
    public float totalSpeed = 5f;
    public float currentPierceCount = 1;
    private float currentLifeTime;
    public float hitRadius = 1f;
    public float acceleration ;
    public int splitCount = 0;
    public float explosionRadius = 0f;

    //collision size
    [SerializeField] private Vector3 baseScale = Vector3.one;
    private bool baseScaleInitialized = false;

    private ObjectPoolManager<ProjectileData, Projectile> objectPoolManager;

    private CancellationTokenSource lifeTimeCts;
    public event Action<GameObject> abilityAction;
    public event Action<GameObject> abilityRelease;
    public event Action<float> damageEvent;

    //super special effect
    public int towerReinforceLevel = 0;
    public System.Collections.Generic.List<int> towerAbilities;
    public float damageMultiplier = 1.0f;
    public int hitCount = 0;

    private bool isFinish = false;
    public bool IsFinish { get => isFinish; set => isFinish = value; }
    public bool IsOtherUser { get; set; }

    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    private void Awake()
    {
        if (!baseScaleInitialized)
        {
            baseScale = transform.localScale;
            baseScaleInitialized = true;
        }
        if (trailRenderer == null)
            trailRenderer = GetComponentInChildren<TrailRenderer>();
    }
    private void OnEnable()
    {
        if(trailRenderer!=null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = true;
            trailRenderer.emitting = true;
        }
    }

    private void OnDisable()
    {
        abilityAction = null;
        if(trailRenderer!=null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
        explosionRadius = 0f;
    }

    private void Update()
    {
        if(isFinish)
        {
            IsOtherUser = false;
            Cancel();
            abilityRelease?.Invoke(gameObject);
            abilityRelease = null;
            damageEvent = null;
            objectPoolManager.Return(poolKeyData, this);
            return;
        }
        
        if (!isFinish && currentLifeTime < projectileData.RemainTime)
        {
            MoveProjectile();
            currentLifeTime += Time.deltaTime;
        }
        else
        {
            // Debug.Log($"[Projectile] DESPAWN reason=isFinish:{isFinish}, life={currentLifeTime:0.00}/{projectileData.RemainTime:0.00}, obj={name}");
            Cancel();
            abilityRelease?.Invoke(gameObject);
            abilityRelease = null;
            objectPoolManager.Return(poolKeyData, this);
        }
    }

    private void OnDestroy()
    {
        Cancel();
        abilityRelease?.Invoke(gameObject);
        abilityRelease = null;
    }

    private void Cancel()
    {
        abilityAction = null;
        lifeTimeCts?.Cancel();
        lifeTimeCts?.Dispose();
        lifeTimeCts = new CancellationTokenSource();
    }

    public void ReturnProjectileToPool()
    {
        Cancel();
        abilityRelease?.Invoke(gameObject);
        abilityRelease = null;
        if(trailRenderer!=null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }
        if (objectPoolManager != null)
            objectPoolManager.Return(poolKeyData, this);
        Debug.Log("[Projectile] ReturnProjectileToPool called");
    }

    private void MoveProjectile()
    {
        totalSpeed += acceleration * Time.deltaTime;

        switch ((ProjectileType)projectileData.AttackType)
        {
            case ProjectileType.Normal:
                transform.position += direction.normalized * totalSpeed * Time.deltaTime;
                break;
            case ProjectileType.Homing:
                if(currentTarget != null && currentTarget.TryGetComponent<Enemy>(out var targetEnemy) && hitEnemies.Contains(targetEnemy))
                {
                    currentTarget = null;
                }

                if(currentTarget != null && !currentTarget.gameObject.activeSelf)
                {
                    currentTarget = null;
                }

                if (currentTarget != null)
                {
                    Vector3 targetDirection = (currentTarget.position - transform.position).normalized;
                    direction = targetDirection;

                    if(direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                    }
                }
                else if (currentTarget != null && !currentTarget.gameObject.activeSelf)
                {
                    currentTarget = null;
                }
                transform.position += direction.normalized * totalSpeed * Time.deltaTime;
                break;
        }
    }

    /// <summary>
    /// Initialize the projectile with data
    /// </summary>
    /// <param name="projectileData">Projectile basic data</param>
    /// <param name="direction">Shooter direction</param>
    /// <param name="isHit">whether or not a hit is judged by the accuracy rate</param>
    public void Initialize(
        ProjectileData projectileData, //Buffed 
        ProjectileData poolKey, //Pool Key(Base Data)
        Vector3 direction, 
        bool isHit, 
        ObjectPoolManager<ProjectileData, Projectile> poolManager,
        Planet planet = null,
        int reinforceLevel = 0,
        System.Collections.Generic.List<int> abilities = null
        )
    {
        this.projectileData = projectileData;
        this.poolKeyData = poolKey;
        this.direction = direction;
        this.isHit = isHit;

        objectPoolManager = poolManager;

        acceleration = projectileData.ProjectileAddSpeed;
        totalSpeed = projectileData.ProjectileSpeed;
        currentPierceCount = projectileData.TargetNum;
        RatePanetration = projectileData.RatePenetration;
        FixedPanetration = projectileData.FixedPenetration;
        damage = projectileData.Attack;
        hitRadius = projectileData.CollisionSize;

        //super special effect
        this.towerReinforceLevel = reinforceLevel;
        this.towerAbilities = abilities != null
            ? new System.Collections.Generic.List<int>(abilities)
            : new System.Collections.Generic.List<int>();

        this.damageMultiplier = 1.0f;
        this.hitCount = 0;

        //hit size
        float baseSize = poolKey != null ? poolKey.CollisionSize : projectileData.CollisionSize;
        float finalSize = projectileData.CollisionSize;
        float sizeMul = finalSize / baseSize;
        transform.localScale = baseScale * sizeMul;

        hitEnemies.Clear();

        Cancel();
        currentLifeTime = 0f;
        isFinish = false;
        if (planet != null)
        {
            this.planet = planet;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagName.Planet) || other.gameObject.CompareTag(TagName.Boss) || other.gameObject.CompareTag(TagName.Projectile)
            || other.gameObject.CompareTag(TagName.DropItem) || other.gameObject.CompareTag(TagName.PatternLine)
            || other.gameObject.CompareTag(TagName.CenterStone) || other.gameObject.CompareTag(TagName.PatternProjectile) ||
            currentPierceCount <= 0)
        {
            return;
        }
        
        var enemy = other.gameObject.GetComponent<Enemy>();
        if(enemy == null)
        {
            enemy = other.gameObject.GetComponentInParent<Enemy>();
        }

        if(enemy != null && hitEnemies.Contains(enemy))
        {
            return;
        }

        var damagable = enemy as IDamagable;

        if (damagable != null && enemy != null && isHit)
        {
            hitEnemies.Add(enemy);

            abilityAction?.Invoke(other.gameObject);

            float pierceMul = GetPierceDamageMultiplier();
            float originalMul = damageMultiplier;

            if (hitCount > 0 && !Mathf.Approximately(pierceMul, 1f))
            {
                damageMultiplier *= pierceMul;
            }

            var damage = CalculateTotalDamage(enemy.Data.Defense);
            damagable.OnDamage(damage);
            ActionEvent(damage);

            damageMultiplier = originalMul;

            hitCount++;
        }

        currentPierceCount--;

        if (currentPierceCount <= 0)
        {
            abilityAction = null;
            isFinish = true;
            damageEvent = null;
        }
    }

    public void ActionEvent(float damage)
    {
        damageEvent?.Invoke(damage);
    }
    
    public float CalculateTotalDamage(float enemyDef)
    {
        float effectiveDamage = damage * damageMultiplier;
        if (effectiveDamage < 0f)
            effectiveDamage = 0f;

        var RatePanetration = Mathf.Clamp(this.RatePanetration, 0f, 100f);
        // Debug.Log(damage);
        var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - FixedPanetration;
        if(totalEnemyDef < 0)
        {
            totalEnemyDef = 0;
        }
        var totalDamage = effectiveDamage * 100f / (100f + totalEnemyDef);
        return totalDamage;
    }

    public void Dispose()
    {
    }

    public void SetHomingTarget(ITargetable target)
    {
        var enemy = target as Enemy;
        if (enemy != null)
        {
            currentTarget = enemy.transform;
        }
    }

    public void ForceFinish()
    {
        isFinish = true;
    }

    private bool HasAbility(int abilityId)
    {
        return towerAbilities != null && towerAbilities.Contains(abilityId);
    }
    private float GetPierceDamageMultiplier()
    {
        const int PIERCE_ABILITY_ID = 200009;

        if (!HasAbility(PIERCE_ABILITY_ID))
            return 1f;

        if (!DataTableManager.IsInitialized)
            return 1f;

        var ra = DataTableManager.RandomAbilityTable?.Get(PIERCE_ABILITY_ID);
        if (ra == null || ra.RandomAbilityType != 1)
            return 1f;

        if (TowerReinforceManager.Instance == null)
            return 1f;

        return TowerReinforceManager.Instance
            .GetFinalSuperValueForAbility(PIERCE_ABILITY_ID, towerReinforceLevel);
    }
}