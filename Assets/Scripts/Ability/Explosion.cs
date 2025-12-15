using UnityEngine;

public class Explosion : MonoBehaviour
{
    private float explosionRadius = 1f;
    private float initRadius = 0.01f;
    private SphereCollider explosionCollider;

    private float explosionTimeInterval = 0.1f;
    private float explosionTimer = 0f;

    private float FixedPanetration = 0f;
    private float RatePanetration = 0f;
    private float damage = 0f;

    private ParticleSystem[] explosionParticles;


    void Awake()
    {
        explosionCollider = GetComponent<SphereCollider>();
        explosionParticles = GetComponentsInChildren<ParticleSystem>();
    }

    public void SetInit(float initRadius, float explosionRadius, ProjectileData projectileData)
    {
        this.initRadius = initRadius;
        this.explosionRadius = explosionRadius;
        damage = projectileData.Attack;
        FixedPanetration = projectileData.FixedPenetration;
        RatePanetration = projectileData.RatePenetration;
    }

    private void Start()
    {
        foreach(var particle in explosionParticles)
        {
            particle.Play();
        }

        if (explosionRadius < initRadius)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        explosionTimer += Time.deltaTime;
        initRadius += Time.deltaTime * (explosionRadius / explosionTimeInterval);
        explosionCollider.transform.localScale = new Vector3(initRadius, initRadius, initRadius);
        if(explosionTimer >= explosionTimeInterval)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var damagable = other.gameObject.GetComponent<IDamagable>();
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (damagable != null && enemy != null)
        {
            damagable.OnDamage(CalculateTotalDamage(enemy.Data.Defense));
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
        var totalDamage = damage * 0.7f * 100f / (100f + totalEnemyDef);
        
        return totalDamage;
    }
}
