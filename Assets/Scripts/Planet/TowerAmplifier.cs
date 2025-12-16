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

    private struct AppliedAbilityInfo
    {
        public int Count;
        public float TotalAmountApplied;
    }
    private readonly Dictionary<TowerAttack, Dictionary<int, AppliedAbilityInfo>> appliedAbilityMap
       = new Dictionary<TowerAttack, Dictionary<int, AppliedAbilityInfo>>();
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
        RefreshAppliedRandomAbilitiesForAllTargets();
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
        if (isAbilitySlot && abilities.Count > 0)
        {
            foreach (var abilityId in abilities)
            {
                ApplyRandomAbilityToTarget(target, abilityId);
            }
        }
        OnBuffTargetsChanged?.Invoke();
    }

    private void ApplyRandomAbilityToTarget(TowerAttack target, int abilityId)
    {
        if (target == null) return;
        if (abilityId <= 0) return;
        if (!AbilityManager.IsInitialized) return;
        if (TowerReinforceManager.Instance == null) return;
        if(!appliedAbilityMap.TryGetValue(target,out var dict))
        {
            dict = new Dictionary<int, AppliedAbilityInfo>();
            appliedAbilityMap[target] = dict;
        }
        dict.TryGetValue(abilityId, out var info);
        target.AddAmplifierAbility(this, abilityId);
        if(info.Count>0)
        {
            RemoveAbilityInstanceFromTower(target, abilityId, info.TotalAmountApplied);
        }
        info.Count += 1;
        float perStack = TowerReinforceManager.Instance.GetFinalPrimaryValueForAbility(abilityId, reinforceLevel);
        float newTotal = perStack * info.Count;

        ApplyAbilityInstanceToTower(target, abilityId, newTotal);

        info.TotalAmountApplied = newTotal;
        dict[abilityId] = info;
        if(abilityId==20004)
        {
            Debug.Log(
                $"[AmpRandom][APPLY] amp={name}, target={target.name}, abilityId={abilityId}, " +
                $"count={info.Count}, totalAmount={info.TotalAmountApplied}, ampReinforce={reinforceLevel}"
            );
        }
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
                var info= kv.Value;
                if (abilityId == 200004)
                {
                    Debug.Log(
                        $"[AmpRandom][REMOVE] amp={name}, target={target.name}, abilityId={abilityId}, count={count}"
                    );
                }

                RemoveAbilityInstanceFromTower(target, abilityId, info.TotalAmountApplied);
                target.RemoveAmplifierAbility(this, abilityId, info.Count);
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
                var info = kv.Value;
                if (abilityId == 200004)
                {
                    Debug.Log(
                        $"[AmpRandom][CLEAR_ALL] amp={name}, target={target.name}, abilityId={abilityId}, " +
                        $"count={info.Count}, amount={info.AmountSnapshot}"
                    );
                }
                RemoveAbilityInstanceFromTower(target, abilityId, info.TotalAmountApplied);
                target.RemoveAmplifierAbility(this, abilityId, info.Count);
            }
        }
        appliedAbilityMap.Clear();
        foreach (var target in buffedTargets)
        {
            if (target == null) continue;
            target.RemoveAmplifierBuff(amplifierTowerData);
        }
        buffedTargets.Clear();
        OnBuffTargetsChanged?.Invoke(); 
    }

    private void OnDestroy()
    {
        ClearAllbuffs();
    }

    private void RefreshAppliedRandomAbilitiesForAllTargets()
    {
        if (!AbilityManager.IsInitialized) return;
        if (TowerReinforceManager.Instance == null) return;
        var targets = new List<TowerAttack>(appliedAbilityMap.Keys);
        foreach(var t in targets)
        {
            if (t == null) continue;
            if (!appliedAbilityMap.TryGetValue(t, out var dict)) continue;
            var abilityIds = new List<int>(dict.Keys);
            foreach(var abilityId in abilityIds)
            {
                var info = dict[abilityId];
                RemoveAbilityInstanceFromTower(t, abilityId, info.TotalAmountApplied);
                float perStack = TowerReinforceManager.Instance.GetFinalPrimaryValueForAbility(abilityId, reinforceLevel);
                float newTotal = perStack * info.Count;
                ApplyAbilityInstanceToTower(t, abilityId, newTotal);
                info.TotalAmountApplied = newTotal;
                dict[abilityId] = info;
                if(abilityId==200004)
                {
                    Debug.Log(
                        $"[AmpRandom][REFRESH] amp={name}, target={t.name}, abilityId={abilityId}, " +
                        $"count={info.Count}, newTotal={info.TotalAmountApplied}, ampReinforce={reinforceLevel}"
                    );
                }
            }
        }
    }

    private void ApplyAbilityInstanceToTower(TowerAttack target, int abilityId, float totalAmount)
    {
        var ab = AbilityManager.GetAbility(abilityId);
        if (ab == null) return;

        float delta = totalAmount - ab.UpgradeAmount;
        if (!Mathf.Approximately(delta, 0f)) ab.StackAbility(delta);
        ab.ApplyAbility(target.gameObject);
        ab.Setting(target.gameObject);
    }

    private void RemoveAbilityInstanceFromTower(TowerAttack target, int abilityId, float totalAmount)
    {
        var ab = AbilityManager.GetAbility(abilityId);
        if (ab == null) return;
        float delta = totalAmount - ab.UpgradeAmount;
        if (!Mathf.Approximately(delta, 0f)) ab.StackAbility(delta);
        ab.RemoveAbility(target.gameObject);
    }

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

        ClearAllbuffs();
        selfIndex = index;
        this.planet = planet;
        SetData(ampData);

        abilities.Clear();
        if (randomAbilityId > 0) abilities.Add(randomAbilityId);
 
        int towerCount = planet.TowerCount;
        if (towerCount <= 0) return;

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
                            }
                        }
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
            Debug.Log(
        $"[AmpRandom][AddAmpTower-BUFF] amp={name}, target={attackTower.name}, slotIndex={slotIndex}"
    );
            ApplyBuff(attackTower, slotIndex);   
        }
        //Random Ability
        foreach (int slotIndex in randomAbilitySlotIndex)
        {
            if (buffedSlotIndex.Contains(slotIndex)) continue;
            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;
            Debug.Log(
        $"[AmpRandom][AddAmpTower-RANDOM] amp={name}, target={attackTower.name}, slotIndex={slotIndex}"
    );
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
        Debug.Log(
       $"[AmpRandom][NewTower] amp={name}, target={newTower.name}, " +
       $"slotIndex={slotIndex}, isBuffSlot={isBuffSlot}, isAbilitySlot={isAbilitySlot}"
   );
        ApplyBuff(newTower, slotIndex);
    }

    //Move Tower
    public void RebuildSlotsForNewIndex(int newSelfIndex, int towerCount)
    {
        bool hasBuff = buffedSlotIndex != null && buffedSlotIndex.Count > 0;
        bool hasRandom = randomAbilitySlotIndex != null && randomAbilitySlotIndex.Count > 0;

        if (!hasBuff && !hasRandom)
        {
            selfIndex = newSelfIndex;
            return;
        }

        int oldSelf = selfIndex;
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

        ClearAllbuffs();
        selfIndex = newSelfIndex;
        buffedSlotIndex.Clear();
        randomAbilitySlotIndex.Clear();

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

        if (planet == null) return;

        foreach (int slotIndex in buffedSlotIndex)
        {
            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;

            ApplyBuff(attackTower, slotIndex);
        }

        foreach (int slotIndex in randomAbilitySlotIndex)
        {
            if (buffedSlotIndex.Contains(slotIndex)) continue;

            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;

            ApplyBuff(attackTower, slotIndex);
        }
    }

    public void ResetLocalBuffStateOnly()
    {
        ClearAllbuffs();
    }

    private IAbility CreateAbilityInstanceWithReinforce(int abilityId)
    {
        var ability = AbilityManager.GetAbility(abilityId);
        if (ability == null) return null;
        float finalPrimary = TowerReinforceManager.Instance.
            GetFinalPrimaryValueForAbility(abilityId, reinforceLevel);

        float delta = finalPrimary - ability.UpgradeAmount;
        if (!Mathf.Approximately(delta, 0f))
            ability.StackAbility(delta);
        
        return ability;
    }

    public void NotifyTargetsAbilityChanged()
    {
        foreach(var kv in appliedAbilityMap)
        {
            var t = kv.Key;
            if (t == null) continue;
            t.RebuildAmplifierAbilityCache(this);
        }
    }
}