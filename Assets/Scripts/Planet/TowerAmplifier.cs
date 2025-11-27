using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerAmplifier : MonoBehaviour
{
    [SerializeField] private AmplifierTowerDataSO amplifierTowerData;
    public AmplifierTowerDataSO AmplifierTowerData => amplifierTowerData;

    private readonly List<TowerAttack> buffedTargets = new List<TowerAttack>();
    private readonly List<int> buffedSlotIndex = new List<int>();
    public IReadOnlyList<int> BuffedSlotIndex => buffedSlotIndex;

    private int selfIndex;
    private Planet planet;

    private List<int> abilities = new List<int>();
    public List<int> Abilities => abilities;

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
            t.SetUpBuff(amplifierTowerData);
        }
        Debug.Log($"[BuffReinforce] level={reinforceLevel}, " +
          $"damageBuff={amplifierTowerData.DamageBuff}, " +
          $"fireRateBuff={amplifierTowerData.FireRateBuff}, " +
          $"projCountBuff={amplifierTowerData.ProjectileCountBuff}");

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

        Debug.Log(
            $"[BuffReinforce] slot={selfIndex}, level={reinforceLevel}, " +
            $"effects={extraEffects.Count}");
    }

    public void ApplyBuff(TowerAttack target) //single target(apply buff)
    {
        if (target == null) return;
        if (amplifierTowerData == null) return;

        target.SetUpBuff(amplifierTowerData); //allow overlap
        
        if(!buffedTargets.Contains(target)) //detect targets
        {
            buffedTargets.Add(target);
        }
    }

    public void RemoveBuff(TowerAttack target) //single target (destory target tower)
    {
        if (target == null) return;
        if (!buffedTargets.Contains(target)) return;

        //Remove All Buff In Slot
        target.SetUpBuff(null);
        buffedTargets.Remove(target);
    }

    public void ClearAllbuffs()//(Destory Buff Tower)
    { 
        foreach(var t in buffedTargets)
        {
            if (t == null) continue;
            t.SetUpBuff(null);
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
        int[] presetBuffSlots=null)
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

                                Debug.Log($"[Amp] self={selfIndex}, offset={offset}, targetIndex={targetIndex}");
                            }
                        }

                        string offsetStr = string.Join(",", presetBuffSlots);
                        string resolvedStr = string.Join(",", resolvedTargets);
                        Debug.Log($"[Amp] self={selfIndex} | offsets=[{offsetStr}] -> targets=[{resolvedStr}]");
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

        //Go Buff-------------------------------------------------
        foreach (int slotIndex in filteredBuffTowers)
        {
            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;
            ApplyBuff(attackTower);
        }
        //--------------------------------------------------------
        Debug.Log($"[Amp] self={selfIndex}, final buffed slots = [{string.Join(",", filteredBuffTowers)}]");
    }
    public void ApplyBuffForNewTower(int slotIndex, TowerAttack newTower)
    {
        if (newTower == null) return;
        if (AmplifierTowerData == null) return;
        if (!buffedSlotIndex.Contains(slotIndex)) return;

        ApplyBuff(newTower);
    }
}