using UnityEngine;

public class AccuracyAbility : TowerAbility
{
    public AccuracyAbility(float amount)
    {
        upgradeAmount = amount;
        abilityType = AbilityApplyType.Fixed;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);
        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower != null)
        {
            Debug.Log($"[AccuracyAbility] Applying {upgradeAmount} to {gameObject.name}, before={tower.AccuracyBuffAdd}");

            tower.AccuracyBuffAdd += upgradeAmount;

            Debug.Log($"[AccuracyAbility] After apply, AccuracyBuffAdd={tower.AccuracyBuffAdd}");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);
        var tower = gameObject.GetComponent<TowerAttack>();
        if (tower != null)
        {
            tower.AccuracyBuffAdd -= upgradeAmount;
        }
    }

    public override string ToString()
    {
        float percent = upgradeAmount; 
        string dir = percent >= 0f ? "Up!!" : "Down!!";
        return $"Hit\nRate\n{percent:+0;-0}%\n{dir}";
    }

    public override IAbility Copy()
    {
        return new AccuracyAbility(upgradeAmount);
    }
}
