using UnityEngine;

public class ProjectileCountUpgradeAbility : TowerAbility
{
    public ProjectileCountUpgradeAbility(float amount)
    {
        upgradeAmount = amount;
        abilityType = AbilityApplyType.Fixed;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.ProjectileCountBuffAdd += Mathf.FloorToInt(upgradeAmount);
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

         var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.ProjectileCountBuffAdd -= Mathf.FloorToInt(upgradeAmount);
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Projectile Count\n{upgradeAmount}\nUp!!";
    }

    public override IAbility Copy()
    {
        return new ProjectileCountUpgradeAbility(upgradeAmount);
    }
}
