using UnityEngine;

public class EffectAbility : IAbility
{
    protected TowerAttack tower;
    protected float upgradeAmount;
    public float UpgradeAmount => upgradeAmount;

    protected AbilityApplyType abilityType = AbilityApplyType.None;
    public AbilityApplyType AbilityType => abilityType;

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
        return new EffectAbility();
    }
}
