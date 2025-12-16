using System;
using UnityEngine;

public class PatternProjectile : MonoBehaviour , IDisposable
{
    private float damage;
    private float moveSpeed;
    private float lifeTime;
    private Vector3 moveDirection;
    public Vector3 MoveDirection {set { moveDirection = value.normalized; } }

    private float spawnTime;
    private int skillId;
    public int SkillId => skillId;
    private PatternSpawner spawner;

    private bool canMove;
    private bool canDealDamage;
    private bool isReturn = false;

    //event
    public event Action<PatternProjectile, float> OnHitByProjectileEvent;
    public event Action<PatternProjectile> OnPlayerHitEvent;

    private ParticleSystem[] particleSystems;

    public void Awake()
    {
        if(particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
    }

    public void Initialize(int id, float damage, float speed, float lifetime, Vector3 direction, PatternSpawner spawner)
    {
        skillId = id;
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

        if(particleSystems != null)
        {
            foreach(var ps in particleSystems)
            {
                if(ps != null)
                {
                    ps.Play();
                }
            }
        }
    }

    private void OnEnable()
    {
        isReturn = false;

        if(particleSystems != null)
        {
            foreach(var ps in particleSystems)
            {
                if(ps != null)
                {
                    ps.Clear();
                }
            }
        }
    }

    private void OnDisable()
    {
        if(particleSystems != null)
        {
            foreach(var ps in particleSystems)
            {
                if(ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }
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
        if (other.CompareTag(TagName.Enemy) || 
        other.CompareTag(TagName.CenterStone) || 
        other.CompareTag(TagName.PatternLine) || 
        other.CompareTag(TagName.PatternProjectile) || 
        other.CompareTag(TagName.Projectile) || 
        isReturn)
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

        if(particleSystems != null)
        {
            foreach(var ps in particleSystems)
            {
                if(ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }
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
