using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    [SerializeField] protected float maxHealth = 100f;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    public virtual float Health { get; set; }
    public bool IsDead { get; protected set; }

    public event Action OnDeathEvent;
    public event Action<float> HpDecreseEvent;
    public event Action<float> DamageEvent;

    protected virtual void OnEnable()
    {
        Health = maxHealth;
        IsDead = false;
    }

    public virtual void OnDamage(float damage)
    {
        if (IsDead)
            return;

        Health -= damage;
        
        DamageEvent?.Invoke(damage);
        HpDecreseEvent?.Invoke(Health);

        if (Health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        IsDead = true;
        OnDeathEvent?.Invoke();
    }
}