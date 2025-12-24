using UnityEngine;

/// <summary>
/// 공속 10% + 공격력 10% + 충돌크기 5%
/// </summary>
public class UnlockAbility200018 : IAbility
{
    private AttackSpeedAbility attackSpeed;
    private AttackUpgradeAbility attackDamage;
    private HItSizeUpgradeAbility hitSize;

    public float UpgradeAmount => 0f;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    public UnlockAbility200018(float attackSpeedValue, float attackDamageValue, float hitSizeValue)
    {
        attackSpeed = new AttackSpeedAbility(attackSpeedValue);
        attackDamage = new AttackUpgradeAbility(attackDamageValue);
        hitSize = new HItSizeUpgradeAbility(hitSizeValue);
    }

    public void ApplyAbility(GameObject gameObject)
    {
        attackSpeed.ApplyAbility(gameObject);
        attackDamage.ApplyAbility(gameObject);
        hitSize.ApplyAbility(gameObject);
    }

    public void RemoveAbility(GameObject gameObject)
    {
        attackSpeed.RemoveAbility(gameObject);
        attackDamage.RemoveAbility(gameObject);
        hitSize.RemoveAbility(gameObject);
    }

    public void Setting(GameObject gameObject)
    {
        attackSpeed.Setting(gameObject);
        attackDamage.Setting(gameObject);
        hitSize.Setting(gameObject);
    }

    public void StackAbility(float amount)
    {
        float ratio = amount / 10f; 

        attackSpeed.StackAbility(amount);
        attackDamage.StackAbility(amount);
        hitSize.StackAbility(amount * 0.5f); 
    }

    public IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(200018);
        return new UnlockAbility200018(
            data.SpecialEffectValue,
            data.SpecialEffect2Value ?? 0f,
            data.SpecialEffect3Value ?? 0f
        );
    }
}