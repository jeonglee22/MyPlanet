using System;
using UnityEngine;

public class PassiveAbility : IAbility
{
    protected TowerAttack tower;
    protected Projectile projectile;
    protected float upgradeAmount;
    public float UpgradeAmount => upgradeAmount;

    protected AbilityApplyType abilityType = AbilityApplyType.None;
    public AbilityApplyType AbilityType => abilityType;

    public PassiveAbility()
    {
    }
    
    public virtual void ApplyAbility(GameObject gameObject)
    {
    }

    public virtual void RemoveAbility(GameObject gameObject)
    {   
    }

    public virtual void Setting(GameObject gameObject)
    {
    }

    public override string ToString()
    {
        return string.Empty;
    }

    public virtual IAbility Copy()
    {
        return new PassiveAbility();
    }

    public virtual void StackAbility(float amount)
    {
        upgradeAmount += amount;
    }
}
