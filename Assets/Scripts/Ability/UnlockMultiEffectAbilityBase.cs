using System.Collections.Generic;
using UnityEngine;

public abstract class UnlockMultiEffectAbilityBase : IAbility, IReinforceSumApplicable
{
    protected readonly int abilityId;

    protected readonly Dictionary<int, IAbility> subByEffectId = new Dictionary<int, IAbility>(4);
    protected readonly List<int> effectOrder = new List<int>(4);

    public float UpgradeAmount => 0f;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    protected UnlockMultiEffectAbilityBase(int abilityId)
    {
        this.abilityId = abilityId;
        BuildFromTable();
    }

    private void BuildFromTable()
    {
        if (!DataTableManager.IsInitialized) return;

        var data = DataTableManager.RandomAbilityTable.Get(abilityId);
        if (data == null) return;

        subByEffectId.Clear();
        effectOrder.Clear();

        TryAddEffect(data.SpecialEffect_ID, data.SpecialEffectValue);

        if (data.SpecialEffect2_ID.HasValue && data.SpecialEffect2_ID.Value != 0)
            TryAddEffect(data.SpecialEffect2_ID.Value, data.SpecialEffect2Value ?? 0f);

        if (data.SpecialEffect3_ID.HasValue && data.SpecialEffect3_ID.Value != 0)
            TryAddEffect(data.SpecialEffect3_ID.Value, data.SpecialEffect3Value ?? 0f);
    }

    private void TryAddEffect(int effectId, float tableValue)
    {
        if (effectId == 0) return;

        var a = CreateAbilityByEffect(effectId, tableValue);
        if (a == null) return;

        subByEffectId[effectId] = a;
        effectOrder.Add(effectId);
    }

    protected virtual IAbility CreateAbilityByEffect(int effectId, float tableValue)
    {
        switch (effectId)
        {
            case 1011001: return new AttackSpeedAbility(tableValue);
            case 1102001: return new AttackUpgradeAbility(tableValue);
            case 1102002: return new ProjectileSpeedAbility(tableValue);
            case 1102004: return new HItSizeUpgradeAbility(tableValue);
            case 1101001: return new PierceUpgradeAbility(tableValue);
            case 1101002: return new ChainUpgradeAbility(tableValue);
            case 1101003: return new ExplosionAbility(tableValue);
            case 1101004: return new HomingUpgradeAbility(tableValue);
            case 1105001: return new AccuracyAbility(tableValue);
            default:
                return null;
        }
    }

    public void ApplyReinforceSum(TowerReinforceManager.RandomAbilityReinforceSum sum)
    {
        if (sum.EffectAdd == null || sum.EffectAdd.Count == 0) return;
        if (!DataTableManager.IsInitialized) return;

        var data = DataTableManager.RandomAbilityTable.Get(abilityId);
        if (data == null) return;

        foreach (var kv in sum.EffectAdd)
        {
            int effectId = kv.Key;
            float addTable = kv.Value;
            if (Mathf.Approximately(addTable, 0f)) continue;

            if (!subByEffectId.TryGetValue(effectId, out var sub) || sub == null)
                continue;

            float baseTable = GetBaseTableValueForEffect(data, effectId);
            float internalAdd = ConvertTableAddToInternalAdd(sub, baseTable, addTable);

            if (!Mathf.Approximately(internalAdd, 0f))
                sub.StackAbility(internalAdd);
        }
    }

    private float GetBaseTableValueForEffect(RandomAbilityData data, int effectId)
    {
        if (data.SpecialEffect_ID == effectId) return data.SpecialEffectValue;
        if (data.SpecialEffect2_ID.HasValue && data.SpecialEffect2_ID.Value == effectId) return data.SpecialEffect2Value ?? 0f;
        if (data.SpecialEffect3_ID.HasValue && data.SpecialEffect3_ID.Value == effectId) return data.SpecialEffect3Value ?? 0f;
        return 0f;
    }

    private float ConvertTableAddToInternalAdd(IAbility sub, float baseTable, float addTable)
    {
        if (!Mathf.Approximately(baseTable, 0f))
        {
            float scale = sub.UpgradeAmount / baseTable;
            if (float.IsNaN(scale) || float.IsInfinity(scale) || Mathf.Approximately(scale, 0f))
                scale = 1f;
            return addTable * scale;
        }

        return addTable * ((sub.AbilityType == AbilityApplyType.Rate) ? 0.01f : 1f);
    }
    protected UnlockMultiEffectAbilityBase CopyInternal()
    {
        var newInstance = CreateNewInstance();

        newInstance.subByEffectId.Clear();
        newInstance.effectOrder.Clear();

        foreach (var effectId in effectOrder)
        {
            if (subByEffectId.TryGetValue(effectId, out var originalSub) && originalSub != null)
            {
                var copiedSub = originalSub.Copy();
                newInstance.subByEffectId[effectId] = copiedSub;
                newInstance.effectOrder.Add(effectId);
            }
        }

        return newInstance;
    }

    protected abstract UnlockMultiEffectAbilityBase CreateNewInstance();

    public void ApplyAbility(GameObject gameObject)
    {
        foreach (var effectId in effectOrder)
            if (subByEffectId.TryGetValue(effectId, out var a) && a != null)
                a.ApplyAbility(gameObject);
    }

    public void RemoveAbility(GameObject gameObject)
    {
        foreach (var effectId in effectOrder)
            if (subByEffectId.TryGetValue(effectId, out var a) && a != null)
                a.RemoveAbility(gameObject);
    }

    public void Setting(GameObject gameObject)
    {
        foreach (var effectId in effectOrder)
            if (subByEffectId.TryGetValue(effectId, out var a) && a != null)
                a.Setting(gameObject);
    }

    public void StackAbility(float amount) { }

    public abstract IAbility Copy();
}