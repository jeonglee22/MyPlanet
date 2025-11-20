using System.Text;
using UnityEngine;

public class SpeedUpgradeAbility : PassiveAbility
{
    public SpeedUpgradeAbility()
    {
        upgradeAmount = 1.2f;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.totalSpeed *= upgradeAmount;
            // Debug.Log("Speed Apply");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.totalSpeed /= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);


    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Speed\n{(upgradeAmount - 1f) * 100}%\nUp!!");

        return sb.ToString();
    }
}
