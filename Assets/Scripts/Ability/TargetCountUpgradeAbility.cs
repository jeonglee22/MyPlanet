using UnityEngine;

public class TargetCountUpgradeAbility : TowerAbility
{
    public TargetCountUpgradeAbility(float amount)
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
            var targetingSystem = towerAttack.TargetingSystem;
            targetingSystem.AddExtraTargetCount(Mathf.FloorToInt(upgradeAmount));
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            var targetingSystem = towerAttack.TargetingSystem;
            targetingSystem.RemoveExtraTargetCount(Mathf.FloorToInt(upgradeAmount));
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Target Count\n{upgradeAmount}\nUp!!";
    }

    public override IAbility Copy()
    {
        return new TargetCountUpgradeAbility(upgradeAmount);
    }
}
