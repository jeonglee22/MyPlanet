using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class AttackUpgradeAbility : PassiveAbility
{
    public AttackUpgradeAbility()
    {
        upgradeAmount = 50f;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage += upgradeAmount;
            // Debug.Log("Damage Apply");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage -= upgradeAmount;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Attack\n{upgradeAmount}\nUp!!");

        return sb.ToString();
    }
}
