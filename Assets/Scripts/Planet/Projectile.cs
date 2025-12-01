using System;
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

    private ObjectPoolManager<ProjectileData, Projectile> objectPoolManager;

    private CancellationTokenSource lifeTimeCts;
    public event Action<GameObject> abilityAction;
    public event Action<GameObject> abilityRelease;

    private bool isFinish = false;
    public bool IsFinish { get => isFinish; set => isFinish = value; }

    private void OnEnable()
    {
        trailRenderer.Clear();
        trailRenderer.enabled = true;
        trailRenderer.emitting = true;
    }

    private void OnDisable()
    {
        abilityAction = null;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        trailRenderer.enabled = false;
    }

    private void Update()
    {
        if(isFinish)
        {
            Cancel();
            abilityRelease?.Invoke(gameObject);
            abilityRelease = null;
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
            Debug.Log($"[Projectile] DESPAWN reason=isFinish:{isFinish}, life={currentLifeTime:0.00}/{projectileData.RemainTime:0.00}, obj={name}");

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
                if (currentTarget != null && currentTarget.gameObject.activeSelf)
                {
                    Vector3 targetDirection = (currentTarget.position - transform.position).normalized;
                    direction = targetDirection;
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
        ObjectPoolManager<ProjectileData, Projectile> poolManager)
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

        Cancel();
        currentLifeTime = 0f;
        isFinish = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagName.Planet) || other.gameObject.CompareTag(TagName.Boss) || other.gameObject.CompareTag(TagName.Projectile)
            || other.gameObject.CompareTag(TagName.DropItem) || other.gameObject.CompareTag(TagName.PatternLine)
            || other.gameObject.CompareTag(TagName.CenterStone) ||
            currentPierceCount <= 0)
        {
            return;
        }
        
        var damagable = other.gameObject.GetComponent<IDamagable>();
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (damagable != null && enemy != null && isHit)
        {
            abilityAction?.Invoke(other.gameObject);
            damagable.OnDamage(CalculateTotalDamage(enemy.Data.Defense));
        }

        currentPierceCount--;

        if (currentPierceCount <= 0)
        {
            abilityAction = null;
            isFinish = true;
        }
    }
    
    public float CalculateTotalDamage(float enemyDef)
    {
        var RatePanetration = Mathf.Clamp(this.RatePanetration, 0f, 100f);
        // Debug.Log(damage);
        var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - FixedPanetration;
        if(totalEnemyDef < 0)
        {
            totalEnemyDef = 0;
        }
        var totalDamage = damage * 100f / (100f + totalEnemyDef);
        
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
}