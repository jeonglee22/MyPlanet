using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    [SerializeField] protected float maxHealth = 100f;
    public float MaxHealth { get => maxHealth; }

    public float Health { get; protected set; }
    public bool IsDead { get; protected set; }

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

        Health -= damage;
        HpDecreseEvent?.Invoke(Health);

        if (Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        IsDead = true;
        OnDeathEvent?.Invoke();
    }
}
