using UnityEngine;

public class PierceUpgradeAbility : PassiveAbility
{
    public PierceUpgradeAbility(float amount)
    {
        upgradeAmount = amount;
        abilityType = AbilityApplyType.Fixed;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.currentPierceCount += upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.currentPierceCount -= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Pierce\n{upgradeAmount}\nUp!!";
    }

    public override IAbility Copy()
    {
        return new PierceUpgradeAbility(upgradeAmount);
    }
}
