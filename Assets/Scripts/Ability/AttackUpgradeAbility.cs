using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class AttackUpgradeAbility : PassiveAbility
{
    public AttackUpgradeAbility(float amount)
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
            towerAttack.DamageBuffMMul += upgradeAmount;
            // Debug.Log("Damage Apply");
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);

        // var projectile = gameObject.GetComponent<Projectile>();
        // if (projectile != null)
        // {
        //     projectile.damage -= projectile.projectileData.Attack * upgradeAmount;
        // }
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Attack\n{upgradeAmount * 100}%\nUp!!");

        return sb.ToString();
    }

    public override IAbility Copy()
    {
        return new AttackUpgradeAbility(upgradeAmount * 100);
    }
}
