using UnityEngine;

public class Enemy : LivingEntity
{
    private EnemyMovement movement;
    [SerializeField] private EnemyData data;

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.OnDamage(10f);
        }
    }

    protected override void OnDamage(float damage)
    {
        base.OnDamage(damage);
    }
    
    protected override void Die()
    {
        base.Die();
        
        Destroy(gameObject);
    }
}
