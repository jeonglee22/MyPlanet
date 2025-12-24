using UnityEngine;

/// <summary>
/// Æø¹ß¹üÀ§ 10 + °üÅëÈ½¼ö 2
/// </summary>
public class UnlockAbility200022 : IAbility
{
    private ExplosionAbility explosion;
    private PierceUpgradeAbility pierce;

    public float UpgradeAmount => 0f;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    public UnlockAbility200022(float explosionValue, float pierceValue)
    {
        explosion = new ExplosionAbility(explosionValue);
        pierce = new PierceUpgradeAbility(pierceValue);
    }

    public void ApplyAbility(GameObject gameObject)
    {
        explosion.ApplyAbility(gameObject);
        pierce.ApplyAbility(gameObject);
    }

    public void RemoveAbility(GameObject gameObject)
    {
        explosion.RemoveAbility(gameObject);
        pierce.RemoveAbility(gameObject);
    }

    public void Setting(GameObject gameObject)
    {
        explosion.Setting(gameObject);
        pierce.Setting(gameObject);
    }

    public void StackAbility(float amount)
    {
        explosion.StackAbility(amount);
        pierce.StackAbility(amount * 0.2f);
    }

    public IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(200022);
        return new UnlockAbility200022(
            data.SpecialEffectValue,
            data.SpecialEffect2Value ?? 0f
        );
    }
}