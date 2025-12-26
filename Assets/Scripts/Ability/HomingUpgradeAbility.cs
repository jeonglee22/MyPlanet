using UnityEngine;

public class HomingUpgradeAbility : TowerAbility
{
    public HomingUpgradeAbility(float amount)
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
            towerAttack.NewProjectileAttackType = (int) ProjectileType.Homing;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);
        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            var currentData = towerAttack.BaseProjectileData;
            if (currentData != null)
            {
                towerAttack.NewProjectileAttackType = (int)currentData.AttackType;
            }
            else
            {
                towerAttack.NewProjectileAttackType = (int)ProjectileType.Normal;
            }
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Homing\nAttack!!";
    }

    public override IAbility Copy()
    {
        return new HomingUpgradeAbility(upgradeAmount);
    }
}
