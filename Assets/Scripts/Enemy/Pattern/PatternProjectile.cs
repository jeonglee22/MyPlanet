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

    private Vector3 velocity;
    private Vector3 acceleration;
    private bool useAcceleration = false;

    private Enemy owner;

    public void Awake()
    {
        if(particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
    }

    public void Initialize(int id, float damage, float speed, float lifetime, Vector3 direction, PatternSpawner spawner, Enemy owner)
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

        useAcceleration = false;
        velocity = direction.normalized * speed;

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

        this.owner = owner;
    }

    public void Initialize(int id, float damage, Vector3 initialVelocity, Vector3 accel, float lifetime, PatternSpawner spawner)
    {
        skillId = id;
        this.damage = damage;
        lifeTime = lifetime;
        this.spawner = spawner;

        spawnTime = Time.time;
        canMove = true;
        canDealDamage = true;

        useAcceleration = true;
        velocity = initialVelocity;
        acceleration = accel;

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

        if(useAcceleration)
        {
            velocity += acceleration * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

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
            var planet = damagable as Planet;
            OnPlayerHitEvent?.Invoke(this);

            if (canDealDamage)
            {
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
            }

            if(OnHitByProjectileEvent == null)
            {
                ReturnToPool();
            }
        }
    }

    public float CalculateTotalDamage(float planetDef, float damage)
    {
        if (damage < 0f)
        {
            damage = 0f;
        }

        var RatePanetration = Mathf.Clamp(owner.RatePenetrate, 0f, 100f);
        // Debug.Log(damage);
        var totalPlanetDef = planetDef * (1 - RatePanetration / 100f) - owner.FixedPenetrate;
        if(totalPlanetDef < 0)
        {
            totalPlanetDef = 0;
        }
        var totalDamage = damage * 100f / (100f + totalPlanetDef);
        
        return totalDamage;
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
