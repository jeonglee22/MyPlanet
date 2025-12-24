using UnityEngine;

public class ProjectileSpeedAbility : TowerAbility
{
    public ProjectileSpeedAbility(float amount)
    {
        upgradeAmount = amount / 100f; // 6% ¡æ 0.06
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);
        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower != null)
        {
            tower.AddProjectileSpeedFromAbilitySource(upgradeAmount);
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);
        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower != null)
        {
            tower.RemoveProjectileSpeedFromAbilitySource(upgradeAmount);
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Projectile\nSpeed\n{upgradeAmount * 100}%\nUp!!";
    }

    public override IAbility Copy()
    {
        return new ProjectileSpeedAbility(upgradeAmount * 100);
    }
}