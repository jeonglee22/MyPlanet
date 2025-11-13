using UnityEngine;

public class AccelationUpgradeAbility : PassiveAbility
{
    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.acceleration -= 5f;
            Debug.Log(projectile.acceleration);
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.acceleration += 5f;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);


    }
}