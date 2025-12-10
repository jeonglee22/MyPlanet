using UnityEngine;

public class FixedPanetrationUpgradeAbility : PassiveAbility
{
    public FixedPanetrationUpgradeAbility(float amount)
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
            towerAttack.FixedPenetrationBuffAdd += upgradeAmount;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
            towerAttack.FixedPenetrationBuffAdd -= upgradeAmount;
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Fixed\nPanetration\n{upgradeAmount}\nUp!!";
    }

    public override IAbility Copy()
    {
        return new FixedPanetrationUpgradeAbility(upgradeAmount);
    }
}
