using UnityEngine;

public class FixedPanetrationUpgradeAbility : PassiveAbility
{
    public FixedPanetrationUpgradeAbility()
    {
        upgradeAmount = 30f;
        abilityType = AbilityApplyType.Fixed;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.FixedPanetration += upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.FixedPanetration -= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Fixed\nPanetration\n{upgradeAmount}\nUp!!";
    }
}
