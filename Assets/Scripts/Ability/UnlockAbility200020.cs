using UnityEngine;

/// <summary>
/// 공격력 20% + 투사체속도 6%
/// </summary>
public class UnlockAbility200020 : IAbility
{
    private AttackUpgradeAbility attackDamage;
    private ProjectileSpeedAbility projectileSpeed;

    public float UpgradeAmount => 0f;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    public UnlockAbility200020(float attackDamageValue, float projectileSpeedValue)
    {
        attackDamage = new AttackUpgradeAbility(attackDamageValue);
        projectileSpeed = new ProjectileSpeedAbility(projectileSpeedValue);
    }

    public void ApplyAbility(GameObject gameObject)
    {
        attackDamage.ApplyAbility(gameObject);
        projectileSpeed.ApplyAbility(gameObject);
    }

    public void RemoveAbility(GameObject gameObject)
    {
        attackDamage.RemoveAbility(gameObject);
        projectileSpeed.RemoveAbility(gameObject);
    }

    public void Setting(GameObject gameObject)
    {
        attackDamage.Setting(gameObject);
        projectileSpeed.Setting(gameObject);
    }

    public void StackAbility(float amount)
    {
        attackDamage.StackAbility(amount);
        projectileSpeed.StackAbility(amount * 0.3f);
    }

    public IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(200020);
        return new UnlockAbility200020(
            data.SpecialEffectValue,
            data.SpecialEffect2Value ?? 0f
        );
    }
}