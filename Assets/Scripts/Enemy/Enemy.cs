using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : LivingEntity
{
    private EnemyMovement movement;
    private List<EnemyAbility> abilities = new List<EnemyAbility>();
    private EnemyData data;

    protected override void OnEnable()
    {
        base.OnEnable();

        movement = GetComponent<EnemyMovement>();

        OnDeathEvent += SpawnManager.Instance.OnEnemyDied;
    }

    protected void Oestroy()
    {
        OnDeathEvent -= SpawnManager.Instance.OnEnemyDied;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            return;
        }
        
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.OnDamage(data.damage);
        }
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
    }

    protected override void Die()
    {
        base.Die();

        Destroy(gameObject);
    }

    public void Initialize(EnemyData enemyData, Vector3 targetDirection)
    {
        data = enemyData;
        maxHealth = data.maxHealth;
        Health = maxHealth;

        AddMovementComponent(data.movementType, data.speed, targetDirection);
        AddAbilityComponents(data.abilityTypes);
    }

    private void AddMovementComponent(MovementType type, float speed, Vector3 targetDirection)
    {
        switch (type)
        {
            case MovementType.StraightDown:
                movement = gameObject.AddComponent<StraightDownMovement>();
                break;
            case MovementType.TargetDirection:
                movement = gameObject.AddComponent<TargetDirectionMovement>();
                break;
        }

        movement.Initialize(speed, targetDirection);
    }
    
    private void AddAbilityComponents(AbilityType[] types)
    {
        if (types == null || types.Length == 0)
        {
            return;
        }
        
        foreach(var type in types)
        {
            EnemyAbility ability = null;
            switch (type)
            {
                case AbilityType.SplitOnDeath:
                    ability = gameObject.AddComponent<SplitDeathAbility>();
                    break;
            }

            if(ability != null)
            {
                ability.Initialize(this, data);
                abilities.Add(ability);
            }
        }
    }
}
