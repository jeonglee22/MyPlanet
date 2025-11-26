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
        
        abilityDict.Add((int)AbilityId.Accuracy, new AttackUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.AttackDamage).SpecialEffectValue));
        abilityDict.Add((int)AbilityId.AttackSpeedOneTarget, new AttackUpgradeAbility(DataTableManager.RandomAbilityTable.Get((int)AbilityId.AttackDamage).SpecialEffectValue));
        
    }

    public static int GetRandomAbility()
    {
        var count = abilityDict.Count;

        if(count == 0)
            return -1;

        var index = Random.Range(0, count);
        var keys = new List<int>(abilityDict.Keys);
        return (int)AbilityId.Hitscan;
        // return keys[index];
    }

    public static IAbility GetAbility(int id)
    {
        if (abilityDict.ContainsKey(id))
            return abilityDict[id].Copy();
        
        return null;
    }
}
