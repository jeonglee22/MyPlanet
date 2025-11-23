using UnityEngine;

public class AttackSpeedAbility : TowerAbility
{
    public AttackSpeedAbility(float amount)
    {
        upgradeAmount = amount / 100f;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.fireRateBuffMul += upgradeAmount;
            // Debug.Log("Damage Apply");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        var towerAttack = gameObject.GetComponent<TowerAttack>();
        if (towerAttack != null)
        {
            towerAttack.fireRateBuffMul -= upgradeAmount;
            // Debug.Log("Damage Apply");
        }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Attack Speed\n{upgradeAmount * 100}%\nUp!!";
    }

    public override IAbility Copy()
    {
        return new AttackSpeedAbility(upgradeAmount * 100);
    }
}
