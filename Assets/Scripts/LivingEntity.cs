using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    [SerializeField] protected float maxHealth = 100f;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    public float Health { get; set; }
    public bool IsDead { get; protected set; }

    protected float shield;
    public float Shield { get => shield; set => shield = value; }

    public event Action OnDeathEvent;
    public event Action<float> HpDecreseEvent;

    protected virtual void OnEnable()
    {
        Health = maxHealth;
        IsDead = false;
    }

    public virtual void OnDamage(float damage)
    {
        if (IsDead)
            return;

        float remainingDamage = damage;

        if(shield > 0)
        {
            if(remainingDamage <= shield)
            {
                shield -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                remainingDamage -= shield;
                shield = 0;
            }
        }

        if(remainingDamage > 0)
        {
            Health -= remainingDamage;
        }
        
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