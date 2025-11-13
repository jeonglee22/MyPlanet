using Unity.VisualScripting;
using UnityEngine;

public class SideMovingAbility : PassiveAbility
{
    public SideMovingAbility()
    {
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }
}
