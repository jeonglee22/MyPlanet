using UnityEngine;

public class DurationUpgradeAbility : PassiveAbility
{
    public DurationUpgradeAbility(float amount)
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
            projectile.projectileData.RemainTime += upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.projectileData.RemainTime -= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"RemainTime\n{upgradeAmount}s\nUp!!";
    }

    public override IAbility Copy()
    {
        return new DurationUpgradeAbility(upgradeAmount);
    }
}
