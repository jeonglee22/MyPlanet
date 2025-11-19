using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerAmplifier : MonoBehaviour
{
    [SerializeField] private AmplifierTowerDataSO amplifierTowerData;
    public AmplifierTowerDataSO AmplifierTowerData => amplifierTowerData;

    private readonly List<TowerAttack> buffedTargets = new List<TowerAttack>();

    private int selfIndex;
    private Planet planet;

    public void SetData(AmplifierTowerDataSO data)
    {
        amplifierTowerData = data;
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

    internal void AddAmpTower(AmplifierTowerDataSO ampData, int index, Planet planet)
    {
        if (ampData == null || planet == null) return;

        amplifierTowerData = ampData;
        selfIndex = index;
        this.planet = planet;

        int towerCount = planet.TowerCount;
        if (towerCount <= 0) return;

        //Candidate Buffed Tower: Attack Tower-------------------
        List<int> buffAbleTowers = new List<int>();

        for (int i = 0; i < towerCount; i++)
        {
            if (i == selfIndex) continue;
            var attackTower = planet.GetAttackTowerToAmpTower(i);
            if (ampData.OnlyAttackTower && attackTower == null) continue;
            if (attackTower == null) continue;
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
                    if (buffAbleTowers.Count == 0) break;

                    int finalBuffedSlotCount = Mathf.Min(ampData.FixedBuffedSlotCount, buffAbleTowers.Count);

                    for (int n = 0; n < finalBuffedSlotCount; n++)
                    {
                        int randIndex = UnityEngine.Random.Range(0, buffAbleTowers.Count);
                        int slotIndex = buffAbleTowers[randIndex];
                        buffAbleTowers.Add(slotIndex);
                        //buffAbleTowers.RemoveAt(randIndex); // 중복 방지
                    }
                    break;
                }

            case AmplifierTargetMode.LeftNeighbor:
                {
                    int leftIndex = (selfIndex - 1 + towerCount) % towerCount;
                    if (leftIndex < 0 || leftIndex >= towerCount) break;

                    var attackTower = planet.GetAttackTowerToAmpTower(leftIndex);
                    if (attackTower != null) buffAbleTowers.Add(leftIndex);

                    break;
                }
        }
        //--------------------------------------------------------

        //Go Buff-------------------------------------------------
        foreach (int slotIndex in buffAbleTowers)
        {
            var attackTower = planet.GetAttackTowerToAmpTower(slotIndex);
            if (attackTower == null) continue;
            ApplyBuff(attackTower);
        }
        //--------------------------------------------------------
    }
}
