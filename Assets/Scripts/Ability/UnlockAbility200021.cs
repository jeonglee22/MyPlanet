using UnityEngine;

/// <summary>
/// 집탄율 10 + 유도
/// </summary>
public class UnlockAbility200021 : IAbility
{
    private AccuracyAbility accuracy;
    private HomingUpgradeAbility homing;

    public float UpgradeAmount => 0f;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    public UnlockAbility200021(float accuracyValue, float homingValue)
    {
        Debug.Log($"[UnlockAbility200021] Constructor - accuracyValue={accuracyValue}, homingValue={homingValue}");

        accuracy = new AccuracyAbility(accuracyValue);
        homing = new HomingUpgradeAbility(homingValue);
    }

    public void ApplyAbility(GameObject gameObject)
    {
        Debug.Log($"[UnlockAbility200021] ApplyAbility to {gameObject.name}");

        accuracy.ApplyAbility(gameObject);
        Debug.Log($"[UnlockAbility200021] -> Accuracy applied");

        homing.ApplyAbility(gameObject);
        Debug.Log($"[UnlockAbility200021] -> Homing applied");
    }

    public void RemoveAbility(GameObject gameObject)
    {
        accuracy.RemoveAbility(gameObject);
        homing.RemoveAbility(gameObject);
    }

    public void Setting(GameObject gameObject)
    {
        accuracy.Setting(gameObject);
        homing.Setting(gameObject);
    }

    public void StackAbility(float amount)
    {
        accuracy.StackAbility(amount);
        // homing은 스택 안 함 (on/off 능력)
    }

    public IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(200021);
        return new UnlockAbility200021(
            data.SpecialEffectValue,
            data.SpecialEffect2Value ?? 0f
        );
    }
}