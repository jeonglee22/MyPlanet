using UnityEngine;

public class TowerAbility : IAbility
{
    protected TowerAttack tower;
    protected float upgradeAmount;

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
}
