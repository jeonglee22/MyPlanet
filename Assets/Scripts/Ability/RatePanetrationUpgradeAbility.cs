using UnityEngine;

public class RatePanetrationUpgradeAbility : PassiveAbility
{
    public RatePanetrationUpgradeAbility()
    {
        upgradeAmount = 1.4f;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.RatePanetration *= upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.RatePanetration /= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Rate\nPanetration\n{(upgradeAmount - 1f) * 100}%\nUp!!";
    }
}
