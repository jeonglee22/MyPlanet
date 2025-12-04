using System.Collections.Generic;
using UnityEngine;

public class HItSizeUpgradeAbility : PassiveAbility
{
    public HItSizeUpgradeAbility(float amount)
    {
        upgradeAmount = amount / 100f;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower != null)
        {
            tower.AddHitRadiusFromAbilitySource(upgradeAmount);
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower != null)
        {
            tower.RemoveHitRadiusFromAbilitySource(upgradeAmount);
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        float percent = upgradeAmount * 100f;
        string dir = percent >= 0f ? "Up!!" : "Down!!";
        return $"Hit\nSize\n{percent:+0;-0}%\n{dir}";
    }

    public override IAbility Copy()
    {
        return new HItSizeUpgradeAbility(upgradeAmount * 100);
    }
}