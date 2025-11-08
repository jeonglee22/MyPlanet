using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class LivingEntity : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;

    public float Health { get; protected set; }
    public bool IsDead { get; protected set; }

    public event Action OnDeathEvent;

    protected virtual void OnEnable()
    {
        Health = maxHealth;
        IsDead = false;
    }

    protected virtual void OnDamage(float damage)
    {
        if (IsDead)
            return;

        Health -= damage;

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
