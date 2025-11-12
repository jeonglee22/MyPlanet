using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    public ProjectileData projectileData;
    public Vector3 direction;
    public bool isHit;

    private Transform currentTarget; //Type: Homing

    //Projectile Data
    public float damage = 10f;
    public float panetration = 0f;
    public float totalSpeed = 5f;
    public int currentPierceCount = 1;
    private float currentLifeTime;
    public float hitRadius = 10f;

    private IObjectPool<Projectile> pool;

    private CancellationTokenSource lifeTimeCts;
    public event Action<GameObject> abilityAction;
    public event Action<GameObject> abilityRelease;

    private void Update()
    {
        if (currentLifeTime < projectileData.lifeTime)
        {
            MoveProjectile();
            currentLifeTime += Time.deltaTime;
        }
        else
        {
            Cancel();
            pool?.Release(this);
        }
    }

    private void OnDestroy()
    {
        Cancel();
    }
    
    public void SetPool(IObjectPool<Projectile> pool)
    {
        this.pool = pool;
    }

    private void Cancel()
    {
        lifeTimeCts?.Cancel();
        lifeTimeCts?.Dispose();
        lifeTimeCts = new CancellationTokenSource();
    }

    private void MoveProjectile()
    {
        totalSpeed += projectileData.acceleration * Time.deltaTime;

        switch (projectileData.projectileType)
        {
            case ProjectileType.Normal:
                transform.position += direction.normalized * totalSpeed * Time.deltaTime;
                break;
        }
    }

    void OnDisable()
    {
        abilityRelease?.Invoke(gameObject);
        abilityRelease = null;
    }

    /// <summary>
    /// Initialize the projectile with data
    /// </summary>
    /// <param name="projectileData">Projectile basic data</param>
    /// <param name="direction">Shooter direction</param>
    /// <param name="isHit">whether or not a hit is judged by the accuracy rate</param>
    public void Initialize(ProjectileData projectileData, Vector3 direction, bool isHit)
    {
        this.projectileData = projectileData;
        this.direction = direction;
        this.isHit = isHit;

        totalSpeed = projectileData.speed;
        currentPierceCount = projectileData.targetNumber;

        Cancel();
        currentLifeTime = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagName.Planet) || other.gameObject.CompareTag(TagName.Projectile)
            || other.gameObject.CompareTag(TagName.DropItem))
        {
            return;
        }
        
        var damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            abilityAction?.Invoke(gameObject);
            damagable.OnDamage(damage);
            Debug.Log(damage);
            abilityAction = null;
        }

        currentPierceCount--;

        if (currentPierceCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
