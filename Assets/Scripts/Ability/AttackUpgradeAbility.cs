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
            projectile.totalSpeed += 100f;
            Debug.Log(projectile.totalSpeed);
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.totalSpeed -= 100f;
            Debug.Log(projectile.totalSpeed);
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }
}
