using Unity.VisualScripting;
using UnityEngine;

public class AttackUpgradeAbility : PassiveAbility
{
    public AttackUpgradeAbility()
    {
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage += 100f;
            // Debug.Log("Damage Apply");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage -= 100f;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }
}
