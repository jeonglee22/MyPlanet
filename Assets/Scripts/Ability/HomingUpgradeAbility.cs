using UnityEngine;

public class HomingUpgradeAbility : TowerAbility
{
    private int initAttackType;

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
            initAttackType = (int)towerAttack.BaseProjectileData.AttackType;
            // Debug.Log("Before Ability" + initAttackType);
            towerAttack.NewProjectileAttackType = (int) ProjectileType.Homing;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.NewProjectileAttackType = initAttackType;
            // Debug.Log("Init Homing" + initAttackType);
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
