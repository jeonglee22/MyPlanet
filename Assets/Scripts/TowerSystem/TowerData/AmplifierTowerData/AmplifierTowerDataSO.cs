using System;
using System.Collections.Generic;
using UnityEngine;

public enum AmplifierType
{
    DamageMatrix,   
    ProjectileCore, 
}

public enum AmplifierTargetMode
{
    RandomSlots,   
    LeftNeighbor,  //no use 
}

[CreateAssetMenu(fileName = "AmplifierTowerDataSO", menuName = "Scriptable Objects/AmplifierTowerDataSO")]
public class AmplifierTowerDataSO : ScriptableObject
{
    //Data Tabble
    [SerializeField] private int buffTowerId;
    [SerializeField] private string buffTowerName;
    [SerializeField][Min(1)]
    private int slotNum = 1;
    [SerializeField] private int specialEffectCombinationId;
    [SerializeField] private int randomAbilityGroupId;

    public int BuffTowerId => buffTowerId;
    public string BuffTowerName => buffTowerName;
    public int SpecialEffectCombinationId => specialEffectCombinationId;
    public int RandomAbilityGroupId => randomAbilityGroupId;
    public int FixedBuffedSlotCount => slotNum;

    //Amplifier Tower Action
    [SerializeField] private AmplifierType amplifierType;
    [SerializeField] private AmplifierTargetMode targetMode = AmplifierTargetMode.RandomSlots;
    [SerializeField] private bool onlyAttackTower = true;

    public AmplifierType AmplifierType => amplifierType;
    public AmplifierTargetMode TargetMode => targetMode;
    public bool OnlyAttackTower => onlyAttackTower;

    //Buff Numbers
    [SerializeField] private float damageBuff = 0f;
    [SerializeField] private float fireRateBuff = 1f;
    [SerializeField] private float accelerationBuff = 0f;
    [SerializeField] private float hitRadiusBuff = 0f;
    [SerializeField] private float percentPenetrationBuff = 0f;
    [SerializeField] private float fixedPenetrationBuff = 0f;
    [SerializeField] private int projectileCountBuff = 0;
    [SerializeField] private int targetNumberBuff = 0;
    [SerializeField] private float hitRateBuff = 1f;
    private int[] buffTowerReinforceUpgradeIds;

    public float DamageBuff => damageBuff;
    public float FireRateBuff => fireRateBuff;
    public float AccelerationBuff => accelerationBuff;
    public float HitRadiusBuff => hitRadiusBuff;
    public float PercentPenetrationBuff => percentPenetrationBuff;
    public float FixedPenetrationBuff => fixedPenetrationBuff;
    public int ProjectileCountBuff => projectileCountBuff;
    public int TargetNumberBuff => targetNumberBuff;
    public float HitRateBuff => hitRateBuff;
    public int[] BuffTowerReinforceUpgrade_ID => buffTowerReinforceUpgradeIds;

    //Calculate Buff Tables
    public void ResetBuffValuesFromTables()
    {
        damageBuff = 0f;
        fireRateBuff = 1f;
        accelerationBuff = 0f;
        hitRadiusBuff = 0f;
        percentPenetrationBuff = 0f;
        fixedPenetrationBuff = 0f;
        projectileCountBuff = 0;
        targetNumberBuff = 0;
        hitRateBuff = 1f;
    }

    public void RefreshFromTables() //Runtime
    {
        if (!DataTableManager.IsInitialized) return;

        var buffData = DataTableManager.BuffTowerTable.Get(buffTowerId);
        if (buffData == null) return;

        buffTowerName = buffData.BuffTowerName;
        slotNum = Mathf.Max(1, buffData.SlotNum);
        specialEffectCombinationId = buffData.SpecialEffectCombination_ID;
        randomAbilityGroupId = buffData.RandomAbilityGroup_ID;
        buffTowerReinforceUpgradeIds = buffData.BuffTowerReinforceUpgrade_ID;

        ResetBuffValuesFromTables();

        if (specialEffectCombinationId <= 0) return;

        var combo = DataTableManager.SpecialEffectCombinationTable.Get(specialEffectCombinationId);
        if (combo == null) return;

        ApplyCombinationFromTables(combo, DataTableManager.SpecialEffectTable);
    }

    public void RefreshFromTables( //Editor
        BuffTowerTable buffTable,
        SpecialEffectCombinationTable comboTable,
        SpecialEffectTable effectTable)
    {
        if (buffTable == null || comboTable == null || effectTable == null) return;

        var buffData = buffTable.Get(buffTowerId);
        if (buffData == null) return;

        buffTowerName = buffData.BuffTowerName;
        slotNum = Mathf.Max(1, buffData.SlotNum);
        specialEffectCombinationId = buffData.SpecialEffectCombination_ID;
        randomAbilityGroupId = buffData.RandomAbilityGroup_ID;
        buffTowerReinforceUpgradeIds = buffData.BuffTowerReinforceUpgrade_ID;

        ResetBuffValuesFromTables();

        if (specialEffectCombinationId <= 0) return;

        var combo = comboTable.Get(specialEffectCombinationId);
        if (combo == null) return;

        ApplyCombinationFromTables(combo, effectTable);
    }

    private void ApplyCombinationFromTables(
        SpecialEffectCombinationData combo,
        SpecialEffectTable effectTable)
    {
        ApplySingleEffect(effectTable, combo.SpecialEffect1_ID, combo.SpecialEffect1Value);
        ApplySingleEffect(effectTable, combo.SpecialEffect2_ID, combo.SpecialEffect2Value);
        ApplySingleEffect(effectTable, combo.SpecialEffect3_ID, combo.SpecialEffect3Value);
    }

    private void ApplySingleEffect(SpecialEffectTable effectTable, int effectId, float value)
    {
        if (effectId == 0) return;

        var effect = effectTable.Get(effectId);
        if (effect == null) return;

        var statKind = SpecialEffectMeta.GetStatKind(effectId);
        if (statKind == AmplifierStatKind.None) return;

        bool isPercent = effect.SpecialEffectValueType == 1;

        float asRate = isPercent ? (value / 100f) : value;
        int asInt = Mathf.RoundToInt(value);

        switch (statKind)
        {
            case AmplifierStatKind.AttackSpeed:
                fireRateBuff *= (1f + asRate);
                break;

            case AmplifierStatKind.DamagePercent:
                damageBuff += asRate;
                break;

            case AmplifierStatKind.ProjectileCount:
                projectileCountBuff += asInt;
                break;

            case AmplifierStatKind.TargetCount:
                targetNumberBuff += asInt;
                break;

            case AmplifierStatKind.HitRate:
                hitRateBuff *= (1f + asRate);
                break;

            case AmplifierStatKind.HitRadius:
                hitRadiusBuff += asRate;
                break;

            case AmplifierStatKind.PercentPenetration:
                percentPenetrationBuff += asRate;
                break;

            case AmplifierStatKind.FixedPenetration:
                fixedPenetrationBuff += value;
                break;
        }
    }
    public void ApplyReinforceEffects(
    Dictionary<int, float> effectAddValues,
    float localScale)
    {
        if (effectAddValues == null || effectAddValues.Count == 0) return;
        if (!DataTableManager.IsInitialized) return;

        var effectTable = DataTableManager.SpecialEffectTable;
        if (effectTable == null) return;

        foreach (var kvp in effectAddValues)
        {
            int effectId = kvp.Key;
            float value = kvp.Value * localScale; 

            ApplySingleEffect(effectTable, effectId, value);
        }
    }
}