using UnityEngine;

public class HitScanUpgradeAbility : TowerAbility
{
    public HitScanUpgradeAbility(float amount)
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
            towerAttack.IsHaveHitScanAbility = true;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.IsHaveHitScanAbility = false;
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Hitscan\nAbility!!";
    }

    public override IAbility Copy()
    {
        return new HitScanUpgradeAbility(upgradeAmount);
    }
}
