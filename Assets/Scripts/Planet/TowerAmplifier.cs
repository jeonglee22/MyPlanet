using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerAmplifier : MonoBehaviour
{
    [SerializeField] private AmplifierTowerDataSO amplifierTowerData;
    public AmplifierTowerDataSO AmplifierTowerData => amplifierTowerData;

    private readonly List<TowerAttack> buffedTargets = new List<TowerAttack>();
    public bool HasAppliedBaseBuffs => buffedTargets.Count > 0;

    private readonly List<int> buffedSlotIndex = new List<int>();
    public IReadOnlyList<int> BuffedSlotIndex => buffedSlotIndex;
    public event Action OnBuffTargetsChanged;

    private readonly List<int> randomAbilitySlotIndex = new List<int>();
    public IReadOnlyList<int> RandomAbilitySlotIndex => randomAbilitySlotIndex;

    private int selfIndex;
    public int SelfIndex => selfIndex;
    private Planet planet;

    private List<int> abilities = new List<int>();
    public List<int> Abilities => abilities;

    private readonly Dictionary<TowerAttack, Dictionary<int, int>> appliedAbilityMap
       = new Dictionary<TowerAttack, Dictionary<int, int>>();
    public bool HasAppliedRandomAbilities => appliedAbilityMap.Count > 0;

    //Reinforce Field --------------------------------------
    [Header("Reinforce (Buff Tower)")]
    [SerializeField] private int reinforceLevel = 0;
    public int ReinforceLevel => reinforceLevel;

    [SerializeField] private float reinforceScale = 1f;

    private AmplifierTowerDataSO baseAmpData;
    private AmplifierTowerDataSO runtimeAmpData;
    //------------------------------------------------------
    public void AddAbility(int ability)
    {
        abilities.Add(ability);
    }

    public void SetData(AmplifierTowerDataSO data)
    {
        baseAmpData = data;

        if (baseAmpData == null)
        {
            runtimeAmpData = null;
            amplifierTowerData = null;
            return;
        }

        runtimeAmpData = ScriptableObject.Instantiate(baseAmpData);
        amplifierTowerData = runtimeAmpData;

        RecalculateReinforceBuff();
    }

    public void SetReinforceLevel(int newLevel)
    {
        newLevel = Mathf.Max(0, newLevel);
        if (newLevel == reinforceLevel) return;
        reinforceLevel = newLevel;
        RecalculateReinforceBuff();

        foreach (var t in buffedTargets)
        {
            if (t == null) continue;
            t.RecalculateAmplifierBuffs();
        }
    }

    private void RecalculateReinforceBuff()
    {
        if (!DataTableManager.IsInitialized) return;

        if (baseAmpData == null || runtimeAmpData == null) return;

        //base
        runtimeAmpData.RefreshFromTables();
        if (reinforceLevel <= 0) return;

        //Reinforce
        //get id
        int[] reinforceIds = runtimeAmpData.BuffTowerReinforceUpgrade_ID;
        if (reinforceIds == null || reinforceIds.Length == 0) return;

        //get add value
        var extraEffects =
            TowerReinforceManager.GetBuffAddValuesByIdsStatic(reinforceIds, reinforceLevel);

        if (extraEffects == null || extraEffects.Count == 0) return;

        //add Reinforce Data
        runtimeAmpData.ApplyReinforceEffects(extraEffects, reinforceScale);
    }

    public void ApplyBuff(TowerAttack target, int slotIndex) //single target(apply buff)
    {
        if (target == null) return;
        if (amplifierTowerData == null) return;

        bool isBuffSlot = buffedSlotIndex.Contains(slotIndex);
        bool isAbilitySlot = randomAbilitySlotIndex.Contains(slotIndex);

        if (!isBuffSlot && !isAbilitySlot) return;

        //buff slot
        if (isBuffSlot)
        {
            target.AddAmplifierBuff(amplifierTowerData);

            if (!buffedTargets.Contains(target))
                buffedTargets.Add(target);
        }

        //random ability slot
        if (isAbilitySlot && abilities != null && abilities.Count > 0)
        {
            foreach (var abilityId in abilities)
            {
                target.AddAbility(abilityId);

                var ability = AbilityManager.GetAbility(abilityId);
                if (ability != null)
                {
                    ability.ApplyAbility(target.gameObject);
                    ability.Setting(target.gameObject);
                }
                var raRow = DataTableManager.RandomAbilityTable?.Get(abilityId);
                if (raRow != null)
                {
                    switch (abilityId)
                    {
                        case 200004: 
                            {
                                float add = raRow.SpecialEffectValue; 
                                target.FixedPenetrationBuffAdd += add;
                                break;
                            }
                    }
                }
                if (!appliedAbilityMap.TryGetValue(target, out var dict))
                {
                    dict = new Dictionary<int, int>();
                    appliedAbilityMap[target] = dict;
                }

                if (!dict.ContainsKey(abilityId))
                    dict[abilityId] = 0;

                dict[abilityId]++;
            }
        }
        OnBuffTargetsChanged?.Invoke();
    }

    public void RemoveBuff(TowerAttack target) //single target (destory target tower)
    {
        if (target == null) return;

        //Remove All Buff In Slot
        if (appliedAbilityMap.TryGetValue(target, out var dict))
        {
            foreach (var kv in dict)
            {
                int abilityId = kv.Key;
                int count = kv.Value;

                var ability = AbilityManager.GetAbility(abilityId);

                for (int i = 0; i < count; i++)
                {
                    if (ability != null)
                    {
                        ability.RemoveAbility(target.gameObject);
                    }

                    target.RemoveAbility(abilityId);
                }
                var raRow = DataTableManager.RandomAbilityTable?.Get(abilityId);
                if (raRow != null)
                {
                    switch (abilityId)
                    {
                        case 200004: 
                            {
                                float add = raRow.SpecialEffectValue;
                                target.FixedPenetrationBuffAdd -= add * count;
                                break;
                            }
                    }
                }
            }

            appliedAbilityMap.Remove(target);
        }
        if (buffedTargets.Contains(target))
        {
            buffedTargets.Remove(target);
            target.RemoveAmplifierBuff(amplifierTowerData);
        }

        OnBuffTargetsChanged?.Invoke();
    }

    public void ClearAllbuffs()//(Destory Buff Tower)
    {
        foreach (var kvTarget in appliedAbilityMap)
        {
            var target = kvTarget.Key;
            if (target == null) continue;

            var dict = kvTarget.Value;
            foreach (var kv in dict)
            {
                int abilityId = kv.Key;
                int count = kv.Value;

                var ability = AbilityManager.GetAbility(abilityId);

                for (int i = 0; i < count; i++)
                {
                    if (ability != null)
                    {
                        ability.RemoveAbility(target.gameObject);
                    }
                    target.RemoveAbility(abilityId);
                }
                var raRow = DataTableManager.RandomAbilityTable?.Get(abilityId);
                if (raRow != null)
                {
                    switch (abilityId)
                    {
                        case 200004:
                            {
                                float add = raRow.SpecialEffectValue;
                                target.FixedPenetrationBuffAdd -= add * count;
                                break;
                            }
                    }
                }
            }
        }
        appliedAbilityMap.Clear();
        foreach (var target in buffedTargets)
        {
            if (target == null) continue;
            target.RemoveAmplifierBuff(amplifierTowerData);
        }
        buffedTargets.Clear();
    }

    private void OnDestroy()
    {
        ClearAllbuffs();
    }

    //Planet.SetAmplifierTower -> AddAmpTower
    internal void AddAmpTower(
        AmplifierTowerDataSO ampData, 
        int index, 
        Planet planet,
        int randomAbilityId,
        int[] presetBuffSlots=null,
        int[] presetRandomSlots = null
        )
    {
        if (ampData == null || planet == null) return;

        selfIndex = index;
        this.planet = planet;

        SetData(ampData);

        abilities.Clear();
        if (randomAbilityId > 0) abilities.Add(randomAbilityId);
 
        int towerCount = planet.TowerCount;
        if (towerCount <= 0) return;

        buffedTargets.Clear();
        buffedSlotIndex.Clear();
        randomAbilitySlotIndex.Clear();

        //Candidate Buffed Tower: Attack Tower-------------------
        List<int> buffAbleTowers = new List<int>();

        for (int i = 0; i < towerCount; i++)
        {
            if (i == selfIndex) continue;
            buffAbleTowers.Add(i);
        }
        if (buffAbleTowers.Count == 0) return;
        //--------------------------------------------------------

        //Filtered by Target Mode (Random || LeftIndex)-----------
        List<int> filteredBuffTowers = new List<int>();

        switch (ampData.TargetMode)
        {
            case AmplifierTargetMode.RandomSlots:
                {
                    //Card Random Pick
                    if(presetBuffSlots != null && presetBuffSlots.Length > 0)
                    {
                        List<int> resolvedTargets = new List<int>();

                        for (int i = 0; i < presetBuffSlots.Length; i++)
                        {
                            int offset = presetBuffSlots[i];

                            int targetIndex = selfIndex + offset;

                            targetIndex %= towerCount;
                            if (targetIndex < 0)
                                targetIndex += towerCount;

                            if (targetIndex == selfIndex) continue;
                            if (!filteredBuffTowers.Contains(targetIndex))
                            {
                                filteredBuffTowers.Add(targetIndex);
                                resolvedTargets.Add(targetIndex);
                            }
                        }
                        string offsetStr = string.Join(",", presetBuffSlots);
                        string resolvedStr = string.Join(",", resolvedTargets);
                    }

                    //No preset or no choose
                    if (filteredBuffTowers.Count == 0 && buffAbleTowers.Count > 0)
                    {
                        int finalBuffedSlotCount = Mathf.Min(
                            ampData.FixedBuffedSlotCount,
                            buffAbleTowers.Count
                        );

                        for (int n = 0; n < finalBuffedSlotCount; n++)
                        {
                            int randIndex = UnityEngine.Random.Range(0, buffAbleTowers.Count);
                            int slotIndex = buffAbleTowers[randIndex];
                            filteredBuffTowers.Add(slotIndex);
                            buffAbleTowers.RemoveAt(randIndex);
                        }
                    }
                    break;
                }

            case AmplifierTargetMode.LeftNeighbor:
                {
                    int leftIndex = (selfIndex - 1 + towerCount) % towerCount;
                    if(buffAbleTowers.Contains(leftIndex)) filteredBuffTowers.Add(leftIndex);
                    break;
                }
        }
        //--------------------------------------------------------
        if (filteredBuffTowers.Count == 0) return;
        
        //Remember Buffed Slots
        buffedSlotIndex.AddRange(filteredBuffTowers);

        //RandomBuff----------------------------------------------
        if (presetRandomSlots != null && presetRandomSlots.Length > 0)
        {
            List<int> resolvedRandom = new List<int>();

            for (int i = 0; i < presetRandomSlots.Length; i++)
            {
                int offset = presetRandomSlots[i];
                int targetIndex = selfIndex + offset;

                targetIndex %= towerCount;
                if (targetIndex < 0)
                    targetIndex += towerCount;

                if (targetIndex == selfIndex) continue;
                if (!resolvedRandom.Contains(targetIndex))
                    resolvedRandom.Add(targetIndex);
            }

            randomAbilitySlotIndex.AddRange(resolvedRandom);
        }
        //--------------------------------------------------------
        //Go Buff-------------------------------------------------
        //Buff
        foreach (int slotIndex in buffedSlotIndex)
        {
            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);

            if (attackTower == null) continue;

            ApplyBuff(attackTower, slotIndex);   
        }
        //Random Ability
        foreach (int slotIndex in randomAbilitySlotIndex)
        {
            if (buffedSlotIndex.Contains(slotIndex))
                continue;

            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;
            ApplyBuff(attackTower, slotIndex);   
        }
        //--------------------------------------------------------
    }
    public void ApplyBuffForNewTower(int slotIndex, TowerAttack newTower)
    {
        if (newTower == null) return;
        if (AmplifierTowerData == null) return;

        bool isBuffSlot = buffedSlotIndex.Contains(slotIndex);
        bool isAbilitySlot = randomAbilitySlotIndex.Contains(slotIndex);

        if (!isBuffSlot && !isAbilitySlot) return;

        ApplyBuff(newTower, slotIndex);
    }

    //Move Tower
    // Move Tower
    public void RebuildSlotsForNewIndex(int newSelfIndex, int towerCount)
    {
        bool hasBuff = buffedSlotIndex != null && buffedSlotIndex.Count > 0;
        bool hasRandom = randomAbilitySlotIndex != null && randomAbilitySlotIndex.Count > 0;

        // 버프/랜덤 슬롯이 아예 없으면 인덱스만 갱신하고 끝
        if (!hasBuff && !hasRandom)
        {
            selfIndex = newSelfIndex;
            return;
        }

        int oldSelf = selfIndex;

        // 1) 현재 절대 슬롯 기준으로 상대 오프셋 계산
        List<int> buffOffsets = null;
        if (hasBuff)
        {
            buffOffsets = new List<int>(buffedSlotIndex.Count);
            foreach (var s in buffedSlotIndex)
            {
                int offset = s - oldSelf;
                buffOffsets.Add(offset);
            }
        }

        List<int> randomOffsets = null;
        if (hasRandom)
        {
            randomOffsets = new List<int>(randomAbilitySlotIndex.Count);
            foreach (var s in randomAbilitySlotIndex)
            {
                int offset = s - oldSelf;
                randomOffsets.Add(offset);
            }
        }

        // 2) 기존에 걸린 버프/랜덤능력 전부 제거 (타워 스탯 원상복구)
        ClearAllbuffs();

        // 3) selfIndex를 새 인덱스로 바꾸고, 슬롯 리스트 초기화
        selfIndex = newSelfIndex;
        buffedSlotIndex.Clear();
        randomAbilitySlotIndex.Clear();

        // 4) 새 selfIndex 기준으로 절대 슬롯 재계산 (원형 인덱스)
        if (buffOffsets != null)
        {
            foreach (var offset in buffOffsets)
            {
                int target = newSelfIndex + offset;

                target %= towerCount;
                if (target < 0) target += towerCount;

                if (target == newSelfIndex) continue;
                if (!buffedSlotIndex.Contains(target))
                    buffedSlotIndex.Add(target);
            }
        }

        if (randomOffsets != null)
        {
            foreach (var offset in randomOffsets)
            {
                int target = newSelfIndex + offset;

                target %= towerCount;
                if (target < 0) target += towerCount;

                if (target == newSelfIndex) continue;
                if (!randomAbilitySlotIndex.Contains(target))
                    randomAbilitySlotIndex.Add(target);
            }
        }

        // 5) 새 슬롯 기준으로 다시 버프/랜덤능력 적용
        if (planet == null) return;

        // 기본 버프 슬롯
        foreach (int slotIndex in buffedSlotIndex)
        {
            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;

            ApplyBuff(attackTower, slotIndex);
        }

        // 랜덤능력 슬롯 (버프 슬롯과 겹치지 않는 것만)
        foreach (int slotIndex in randomAbilitySlotIndex)
        {
            if (buffedSlotIndex.Contains(slotIndex))
                continue;

            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;

            ApplyBuff(attackTower, slotIndex);
        }
    }

    public void ResetLocalBuffStateOnly()
    {
        appliedAbilityMap.Clear();
        buffedTargets.Clear();
    }
}