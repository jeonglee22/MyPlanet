using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    public ProjectileData projectileData; //Buffed Data
    private ProjectileData poolKeyData; //BaseDataSO for Key 

    public Vector3 direction;
    public bool isHit;

    private Transform currentTarget; //Type: Homing

    //Projectile Data
    public float damage = 10f;
    public float RatePanetration { get;  set; }
    public float FixedPanetration { get;  set; }
    public float totalSpeed = 5f;
    public int currentPierceCount = 1;
    private float currentLifeTime;
    public float hitRadius = 10f;
    public float acceleration ;

    private ObjectPoolManager<ProjectileData, Projectile> objectPoolManager;

    private CancellationTokenSource lifeTimeCts;
    public event Action<GameObject> abilityAction;
    public event Action<GameObject> abilityRelease;

    private bool isFinish = false;

    private void Update()
    {
        if (!isFinish && currentLifeTime < projectileData.lifeTime)
        {
            MoveProjectile();
            currentLifeTime += Time.deltaTime;
        }
        else
        {
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
        lifeTimeCts?.Cancel();
        lifeTimeCts?.Dispose();
        lifeTimeCts = new CancellationTokenSource();
    }

    private void MoveProjectile()
    {
        totalSpeed += acceleration * Time.deltaTime;

        switch (projectileData.projectileType)
        {
            case ProjectileType.Normal:
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

        acceleration = projectileData.acceleration;
        totalSpeed = projectileData.speed;
        currentPierceCount = projectileData.targetNumber;
        RatePanetration = projectileData.percentPenetration;
        FixedPanetration = projectileData.fixedPanetration;
        damage = projectileData.damage;

        Cancel();
        currentLifeTime = 0f;
        isFinish = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagName.Planet) || other.gameObject.CompareTag(TagName.Projectile)
            || other.gameObject.CompareTag(TagName.DropItem) || other.gameObject.CompareTag("PatternLine"))
        {
            return;
        }

        var damagable = other.gameObject.GetComponent<IDamagable>();
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (damagable != null && enemy != null && isHit)
        {
            abilityAction?.Invoke(other.gameObject);
            damagable.OnDamage(CalculateTotalDamage(enemy.Data.Defense));
            abilityAction = null;
        }

        currentPierceCount--;

        if (currentPierceCount <= 0)
        {
            isFinish = true;
        }
    }
    
    private float CalculateTotalDamage(float enemyDef)
    {
        var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - FixedPanetration;
        if(totalEnemyDef < 0)
        {
            totalEnemyDef = 0;
        }
        var totalDamage = damage - totalEnemyDef;
        
        return totalDamage;
    }
}