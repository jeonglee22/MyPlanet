using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum AbilityApplyType
{
    None = -1,
    Rate,
    Fixed,
}

public class AbilityManager : MonoBehaviour
{
    private static Dictionary<int, IAbility> abilityDict;
    public static Dictionary<int, IAbility> AbilityDict => abilityDict;
    public static bool IsInitialized => abilityDict != null;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => DataTableManager.IsInitialized);

        abilityDict = new Dictionary<int, IAbility>();
        
        // abilityDict.Add(1, new AccelationUpgradeAbility());
        // abilityDict.Add(2, new SpeedUpgradeAbility());
        abilityDict.Add((int)AbilityId.AttackDamage, new AttackUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.AttackDamage).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.AttackSpeed, new AttackSpeedAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.AttackSpeed).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.PercentPenetration, new RatePanetrationUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.PercentPenetration).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.FixedPanetration, new FixedPanetrationUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.FixedPanetration).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Slow, new ParalyzeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Slow).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.CollisionSize, new HItSizeUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.CollisionSize).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Chain, new ChainUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Chain).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Explosion, new ExplosionAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Explosion).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Pierce, new PierceUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Pierce).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Split, new SplitUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Split).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.ProjectileCount, new ProjectileCountUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.ProjectileCount).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Homing, new HomingUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Homing).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Duration, new DurationUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Duration).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.TargetCount, new TargetCountUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.TargetCount).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Hitscan, new HitScanUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Hitscan).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.Accuracy, new AccuracyAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.Accuracy).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.AttackSpeedOneTarget, new AttackSpeedOneTargetAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.AttackDamage).SpecialEffectValue));
    }

    public static int GetRandomAbility()
    {
        var count = abilityDict.Count;

        if(count == 0)
            return -1;

        if(CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int idx = Random.Range(0, count);
            var keys = new List<int>(abilityDict.Keys);
            return keys[idx];
        }

        //weight pick
        List<int> candidateIds = new List<int>(abilityDict.Keys);
        List<float> weights = new List<float>();

        foreach(var id in candidateIds)
        {
            float weight = CollectionManager.Instance.GetWeight(id);
            weights.Add(weight);
        }
        
        return PickRandomFromList(candidateIds, weights);
    }

    public static IAbility GetAbility(int id)
    {
        if (abilityDict.ContainsKey(id))
            return abilityDict[id].Copy();
        
        return null;
    }

    //Pick Random with Weight Helper------------------------------------------
    private static int PickRandomFromList(List<int> candidateIds, List<float> weights = null)
    {
        if (candidateIds == null || candidateIds.Count == 0) return -1;
        if(weights==null||weights.Count!=candidateIds.Count) //no weight
        {
            int idx = Random.Range(0, candidateIds.Count);
            return candidateIds[idx];
        }

        float totalWeight = 0;
        for(int i=0; i<weights.Count; i++)
        {
            totalWeight += Mathf.Max(0, weights[i]);
        }

        float rand = Random.Range(0, totalWeight);
        float cumulative = 0;

        for(int i=0; i<candidateIds.Count; i++)
        {
            cumulative += Mathf.Max(0, weights[i]);
            if (rand < cumulative) return candidateIds[i];
        }
        return candidateIds[candidateIds.Count - 1];
    }
    //------------------------------------------------------------------

    //RandomAbilitygroup + TowerType + Weight---------------------------
    public static int GetRandomAbilityFromGroup(int randomAbilityGroupId, int requiredTowerType, bool useWeight = false)
    {
        var groupRow = DataTableManager.RandomAbilityGroupTable.Get(randomAbilityGroupId);

        if (requiredTowerType >= 0 && groupRow.TowerType != requiredTowerType) return -1;

        var candidateIds = new List<int>();
        List<float> weights = useWeight ? new List<float>() : null;

        foreach (var abilityId in groupRow.RandomAbilityGroupList)
        {
            if (abilityId <= 0) continue;

            var raRow = DataTableManager.RandomAbilityTable.Get(abilityId);
            if (raRow == null) continue;

            candidateIds.Add(abilityId);

            if (useWeight && weights != null)
            {
                if (CollectionManager.Instance != null && CollectionManager.Instance.IsInitialized)
                {
                    weights.Add(CollectionManager.Instance.GetWeight(abilityId));
                }
                else
                {
                    float w = raRow.Weight;
                    if (w <= 0) w = 1f;
                    weights.Add(w);
                }
            }
        }
        return PickRandomFromList(candidateIds, weights);
    }
    //------------------------------------------------------------------
}