using UnityEngine;

public class GunTowerUnLockAbility : PassiveAbility
{
    private float secondUpgradeAmount;
    private float thirdUpgradeAmount;

    public GunTowerUnLockAbility(float amount)
    {
        upgradeAmount = amount / 100f;
        abilityType = AbilityApplyType.Fixed;
    }

    public GunTowerUnLockAbility(float amount1, float amount2, float amount3)
    {
        upgradeAmount = amount1 / 100f;
        secondUpgradeAmount = amount2 / 100f;
        thirdUpgradeAmount = amount3 / 100f;
        abilityType = AbilityApplyType.Fixed;
    }
    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.AddFireRateFromAbilitySource(upgradeAmount * 100f);
            towerAttack.DamageBuffMul += secondUpgradeAmount;
            tower.AddHitRadiusFromAbilitySource(upgradeAmount);
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
