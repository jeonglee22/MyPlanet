using UnityEngine;

public class SpeedUpgradeAbility : PassiveAbility
{
    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.totalSpeed += 10f;
            Debug.Log("Speed Apply");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.totalSpeed -= 10f;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);


    }
}
