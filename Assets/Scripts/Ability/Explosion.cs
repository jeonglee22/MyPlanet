using System;
using UnityEngine;

public class Explosion : MonoBehaviour, IDisposable
{
    private float explosionRadius = 1f;
    private Projectile projectile;
    private float initRadius = 0.01f;
    private SphereCollider explosionCollider;

    private float explosionTimeInterval = 0.1f;
    private float explosionTimer = 0f;

    private float FixedPanetration = 0f;
    private float RatePanetration = 0f;
    private float damage = 0f;
    private float explosionDamageMultiplier = 0.1f;

    private ParticleSystem[] explosionParticles;

    private bool sfxPlayed = false;

    void Awake()
    {
        explosionCollider = GetComponent<SphereCollider>();
        explosionParticles = GetComponentsInChildren<ParticleSystem>();
    }
    private void OnEnable()
    {
        explosionTimer = 0f;
        sfxPlayed = false;

        if (explosionCollider != null)
            explosionCollider.enabled = true;

        if (explosionParticles != null)
        {
            foreach (var p in explosionParticles)
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void SetInit(float initRadius, float explosionRadius, ProjectileData projectileData, Projectile projectile, float damageMultiplier = 0.1f)
    {
        this.initRadius = initRadius;
        this.explosionRadius = explosionRadius;
        this.projectile = projectile;
        damage = projectileData.Attack;
        FixedPanetration = projectileData.FixedPenetration;
        RatePanetration = projectileData.RatePenetration;
        explosionDamageMultiplier = damageMultiplier;

        if (explosionParticles != null)
        {
            foreach (var p in explosionParticles)
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                p.Play(true);
            }
        }
        PlayExplosionSfxOnce();

        if (explosionCollider != null)
            explosionCollider.transform.localScale = Vector3.one * this.initRadius;
    }

/*    private void Start()
    {
        foreach(var particle in explosionParticles)
        {
            particle.Play();
        }
        PlayExplosionSfxOnce();
        if (explosionRadius < initRadius)
        {
            Despawn();
        }
    }*/

    // Update is called once per frame
    private void Update()
    {
        explosionTimer += Time.deltaTime;
        initRadius += Time.deltaTime * (explosionRadius / explosionTimeInterval);
        explosionCollider.transform.localScale = new Vector3(initRadius, initRadius, initRadius);
        if(explosionTimer >= explosionTimeInterval)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var damagable = other.gameObject.GetComponent<IDamagable>();
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (damagable != null && enemy != null)
        {
            var damage = CalculateTotalDamage(enemy.Data.Defense);
            damagable.OnDamage(damage);
            projectile.ActionEvent(damage);
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
        var totalDamage = damage * explosionDamageMultiplier * 100f / (100f + totalEnemyDef);
        return totalDamage;
    }

    private void PlayExplosionSfxOnce()
    {
        if (sfxPlayed) return;
        sfxPlayed = true;

        SoundManager.Instance.PlayExplosionEffect(gameObject.transform.position);
    }
    private void Despawn()
    {
        if (ExplosionPoolManager.Instance != null)
            ExplosionPoolManager.Instance.Return(this);
        else
            gameObject.SetActive(false);
    }

    public void Dispose()
    {
        projectile = null;
        explosionTimer = 0f;

        if (explosionCollider != null)
            explosionCollider.enabled = true;

        if (explosionParticles != null)
        {
            foreach (var p in explosionParticles)
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
