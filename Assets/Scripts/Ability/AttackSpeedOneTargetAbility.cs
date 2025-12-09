using UnityEngine;

public class AttackSpeedOneTargetAbility : TowerAbility
{
    public AttackSpeedOneTargetAbility(float amount)
    {
        upgradeAmount = amount;             
        abilityType = AbilityApplyType.Fixed;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower == null) return;
        tower.AddFireRateFromAbilitySource(upgradeAmount);
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower == null) return;
        tower.RemoveFireRateFromAbilitySource(upgradeAmount);
    }

    public override string ToString()
    {
        float percent = upgradeAmount;
        string dir = percent >= 0f ? "Up!!" : "Down!!";
        return $"Atk\nSpeed\n{percent:+0;-0}%\n{dir}";
    }

    public override IAbility Copy()
    {
        return new AttackSpeedOneTargetAbility(upgradeAmount);
    }
}
