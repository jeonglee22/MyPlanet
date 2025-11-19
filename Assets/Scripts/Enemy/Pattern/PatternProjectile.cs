using UnityEngine;

public class PatternProjectile : MonoBehaviour
{
    private float damage;
    private float moveSpeed;
    private float lifeTime;
    private Vector3 moveDirection;

    private float spawnTime;
    private int patternId;
    private PatternSpawner spawner;

    public void Initialize(int id, float damage, float speed, float lifetime, Vector3 direction, PatternSpawner spawner)
    {
        patternId = id;
        this.damage = damage;
        moveSpeed = speed;
        lifeTime = lifetime;
        moveDirection = direction.normalized;
        this.spawner = spawner;

        spawnTime = Time.time;
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if(Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            return;
        }

        IDamagable damagable = other.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.OnDamage(damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        spawner?.ReturnPatternToPool(this);
    }
}
