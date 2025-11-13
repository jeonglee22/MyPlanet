using System.Text;
using UnityEngine;

public class AccelationUpgradeAbility : PassiveAbility
{
    public AccelationUpgradeAbility()
    {
        upgradeAmount = 5f;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.acceleration -= upgradeAmount;
            // Debug.Log(projectile.acceleration);
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.acceleration += upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);

    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Accelation\n{upgradeAmount}\nUp!!");

        return sb.ToString();
    }
}