using UnityEngine;

public class Enemy : LivingEntity
{
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
