using Unity.VisualScripting;
using UnityEngine;

public class Planet : LivingEntity
{
    private PlanetAttack planetAttack;

    private void Awake()
    {
        planetAttack = GetComponent<PlanetAttack>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.touchCount != 0)
        {
            planetAttack.Shoot(ProjectileType.Normal, transform.forward, true);
        }
#endif
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
