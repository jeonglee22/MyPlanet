using UnityEngine;

public class HItSizeUpgradeAbility : PassiveAbility
{
    public HItSizeUpgradeAbility()
    {
        upgradeAmount = 1.5f;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.gameObject.transform.localScale *= upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.gameObject.transform.localScale /= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Hit\nSize\n{(upgradeAmount - 1f) * 100}%\nUp!!";
    }
}
