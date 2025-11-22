using UnityEngine;

public class RatePanetrationUpgradeAbility : PassiveAbility
{
    public RatePanetrationUpgradeAbility(float amount)
    {
        upgradeAmount = amount / 100f;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.RatePanetration += projectile.projectileData.RatePenetration * upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.RatePanetration -= projectile.projectileData.RatePenetration * upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Rate\nPanetration\n{upgradeAmount * 100f}%\nUp!!";
    }

    public override IAbility Copy()
    {
        return new RatePanetrationUpgradeAbility(upgradeAmount * 100);
    }
}
