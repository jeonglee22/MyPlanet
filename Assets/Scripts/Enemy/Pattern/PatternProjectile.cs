using System;
using UnityEngine;

public class PatternProjectile : MonoBehaviour , IDisposable
{
    [SerializeField] private TrailRenderer trailRenderer;
    private float damage;
    private float moveSpeed;
    private float lifeTime;
    private Vector3 moveDirection;
    public Vector3 MoveDirection {set { moveDirection = value.normalized; } }

    private float spawnTime;
    private int patternId;
    private PatternSpawner spawner;

    private bool canMove;
    private bool canDealDamage;
    private bool isReturn = false;

    //event
    public event Action<PatternProjectile, float> OnHitByProjectileEvent;
    public event Action<PatternProjectile> OnPlayerHitEvent;

    public void Awake()
    {
        if (trailRenderer == null)
            trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    public void Initialize(int id, float damage, float speed, float lifetime, Vector3 direction, PatternSpawner spawner)
    {
        patternId = id;
        this.damage = damage;
        moveSpeed = speed;
        lifeTime = lifetime;
        moveDirection = direction.normalized;
        this.spawner = spawner;

        spawnTime = Time.time;
        canMove = true;
        canDealDamage = true;

        OnHitByProjectileEvent = null;
        OnPlayerHitEvent = null;
    }

    private void OnEnable()
    {
        isReturn = false;
        
        if(trailRenderer!=null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = true;
            trailRenderer.emitting = true;
        }
    }

    private void OnDisable()
    {
        if(trailRenderer!=null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
        transform.position = PatternSpawner.Instance.transform.position;
    }

    private void Update()
    {
        if (!canMove)
        {
            return;
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if(Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagName.Enemy) || isReturn)
        {
            return;
        }

        if (other.CompareTag(ObjectName.Projectile))
        {
            if(OnHitByProjectileEvent != null)
            {
                float projectileDamage = GetProjectileDamage(other);
                OnHitByProjectileEvent.Invoke(this, projectileDamage);
                return;
            }
            else
            {
                ReturnToPool();
                return;
            }
        }

        IDamagable damagable = other.GetComponent<IDamagable>();
        if (damagable != null)
        {
            if (canDealDamage)
            {
                OnPlayerHitEvent?.Invoke(this);
                damagable.OnDamage(damage);
            }

            if(OnHitByProjectileEvent == null)
            {
                ReturnToPool();
            }
        }
    }

    public void ReturnToPool()
    {
        if(isReturn)
        {
            return;
        }
        isReturn = true;

        OnHitByProjectileEvent = null;
        OnPlayerHitEvent = null;
        if(trailRenderer!=null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }
        spawner?.ReturnPatternToPool(this);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetCanDealDamage(bool value)
    {
        canDealDamage = value;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    private float GetProjectileDamage(Collider projectileCollider)
    {
        Projectile projectile = projectileCollider.GetComponent<Projectile>();
        if (projectile != null)
        {
            return projectile.damage;
        }
        return 0f;
    }

    public void Dispose()
    {
    }
}
