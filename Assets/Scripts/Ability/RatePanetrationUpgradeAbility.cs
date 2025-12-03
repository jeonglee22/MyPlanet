using System.Collections.Generic;
using UnityEngine;

public class RatePanetrationUpgradeAbility : PassiveAbility
{
    public RatePanetrationUpgradeAbility(float amount)
    {
        upgradeAmount = amount / 100f;
        abilityType = AbilityApplyType.Rate;
    }

    private readonly Dictionary<TowerAttack, int> _stackMap
        = new Dictionary<TowerAttack, int>();

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower == null) return;

        int stack;
        _stackMap.TryGetValue(tower, out stack);
        stack++;
        _stackMap[tower] = stack;

        float p = Mathf.Clamp01(upgradeAmount);
        float oneMinusP = 1f - p;
        float oneMinusTotal = Mathf.Pow(oneMinusP, stack);   // (1 - p)^n
        float total = 1f - oneMinusTotal;

        tower.PercentPenetrationBuffMul = total;
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower == null) return;

        if (!_stackMap.TryGetValue(tower, out var stack)) return;

        stack--;
        if (stack <= 0)
        {
            _stackMap.Remove(tower);
            tower.PercentPenetrationBuffMul = 0f;
            return;
        }
        _stackMap[tower] = stack;

        float p = Mathf.Clamp01(upgradeAmount);
        float oneMinusP = 1f - p;
        float oneMinusTotal = Mathf.Pow(oneMinusP, stack); //1 - (1 - p)^n
        float total = 1f - oneMinusTotal;

        tower.PercentPenetrationBuffMul = total;
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Rate\nPanetration\n{upgradeAmount * 100f}%\nUp!!";
    }

    public override IAbility Copy()
    {
        return new RatePanetrationUpgradeAbility(upgradeAmount * 100);
    }
}
