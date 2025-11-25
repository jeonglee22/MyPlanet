using UnityEngine;

public class HItSizeUpgradeAbility : PassiveAbility
{
    public HItSizeUpgradeAbility(float amount)
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
            projectile.gameObject.transform.localScale += new Vector3(0.2f,0.2f,0.2f) * upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.gameObject.transform.localScale -= new Vector3(0.2f,0.2f,0.2f) * upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Hit\nSize\n{upgradeAmount * 100f}%\nUp!!";
    }

    public override IAbility Copy()
    {
        return new HItSizeUpgradeAbility(upgradeAmount * 100);
    }
}
