using UnityEngine;

/// <summary>
/// 충돌크기 10% + 연쇄수 3
/// </summary>
public class UnlockAbility200023 : IAbility
{
    private HItSizeUpgradeAbility hitSize;
    private ChainUpgradeAbility chain;

    public float UpgradeAmount => 0f;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    public UnlockAbility200023(float hitSizeValue, float chainValue)
    {
        hitSize = new HItSizeUpgradeAbility(hitSizeValue);
        chain = new ChainUpgradeAbility(chainValue);
    }

    public void ApplyAbility(GameObject gameObject)
    {
        hitSize.ApplyAbility(gameObject);
        chain.ApplyAbility(gameObject);
    }

    public void RemoveAbility(GameObject gameObject)
    {
        hitSize.RemoveAbility(gameObject);
        chain.RemoveAbility(gameObject);
    }

    public void Setting(GameObject gameObject)
    {
        hitSize.Setting(gameObject);
        chain.Setting(gameObject);
    }

    public void StackAbility(float amount)
    {
        hitSize.StackAbility(amount);
        chain.StackAbility(amount * 0.3f);
    }

    public IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(200023);
        return new UnlockAbility200023(
            data.SpecialEffectValue,
            data.SpecialEffect2Value ?? 0f
        );
    }
}