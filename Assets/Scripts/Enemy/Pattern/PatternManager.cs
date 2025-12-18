using System;
using System.Collections.Generic;
using UnityEngine;

public class PatternManager : MonoBehaviour
{
    private Dictionary<int, Func<IPattern>> patternDict;

    private static PatternManager instance;
    public static PatternManager Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        patternDict = new Dictionary<int, Func<IPattern>>();

        patternDict.Add((int)PatternIds.HomingMeteorCluster, () => new MeteorClusterPattern());
        patternDict.Add((int)PatternIds.ChaseMeteorCluster, () => new MeteorClusterPattern());
        patternDict.Add((int)PatternIds.SimpleShot, () => new SimpleShotPattern());
        patternDict.Add((int)PatternIds.DaphnisMeteorClusterSummon, () => new MeteorClusterSummonPattern());
        patternDict.Add((int)PatternIds.DaphnisEleteMeteorClusterSummon, () => new MeteorClusterSummonPattern());
        patternDict.Add((int)PatternIds.TitanMeteorClusterSummon, () => new MeteorClusterSummonPattern());
        patternDict.Add((int)PatternIds.TitanEleteMeteorClusterSummon, () => new MeteorClusterSummonPattern());
        patternDict.Add((int)PatternIds.TitanLazer, () => new LazerPattern());
        patternDict.Add((int)PatternIds.SaturnMeteorRingSummon, () => new MeteorRingSummonPattern());
        patternDict.Add((int)PatternIds.SaturnLazer, () => new RootLazerPattern());
        patternDict.Add((int)PatternIds.SaturnMeteorClusterSummon, () => new MeteorClusterSummonPattern());
        patternDict.Add((int)PatternIds.SaturnEleteMeteorClusterSummon, () => new MeteorClusterSummonPattern());
        patternDict.Add((int)PatternIds.NereidDiaSummon, () => new LateralFrontSummonPattern());
        patternDict.Add((int)PatternIds.NereidReflectShield, () => new ReflectShieldPattern());
        patternDict.Add((int)PatternIds.NeptuneChaseDiaSummon, () => new LateralHommingSummonPattern());
        patternDict.Add((int)PatternIds.NeptuneBigDiaSummon, () => new LateralDownSummonPattern());
        patternDict.Add((int)PatternIds.NeptuneFrontDiaSummon, () => new LateralDownSummonPattern());
        patternDict.Add((int)PatternIds.EliteDiaReflectShield, () => new ReflectShieldPattern());
        patternDict.Add((int)PatternIds.EliteBigDiaReflectShield, () => new ReflectShieldPattern());
        patternDict.Add((int)PatternIds.FireChildHitChangeSpeedChase, () => new HitSpeedBoostPattern());
        patternDict.Add((int)PatternIds.FireEyeShootFire, () => new SimpleShotPattern());
        patternDict.Add((int)PatternIds.BigFireEyeSummonFireChild, () => new SimpleSummonPattern());
        patternDict.Add((int)PatternIds.BigFireEyeSummonFireEye, () => new SimpleSummonPattern());
        patternDict.Add((int)PatternIds.BigFireEyeShootBigFire, () => new SimpleShotPattern());
        patternDict.Add((int)PatternIds.BigFireEyeFirePillar, () => new FirePillarLazerPattern());
        patternDict.Add((int)PatternIds.SunSummonFireChild, () => new SimpleSummonPattern());
        patternDict.Add((int)PatternIds.SunSummonFireEye, () => new SimpleSummonPattern());
        patternDict.Add((int)PatternIds.SunShootFire, () => new ParabolicShotPattern());

    }

    public IPattern GetPattern(int patternId)
    {
        if (patternDict.ContainsKey(patternId))
        {
            return patternDict[patternId]();
        }

        return null;
    }

    public List<IPattern> GetPatterns(List<int> patternIds)
    {
        List<IPattern> patterns = new List<IPattern>();

        foreach (var id in patternIds)
        {
            var pattern = GetPattern(id);
            if (pattern != null)
            {
                patterns.Add(pattern);
            }
        }

        return patterns;
    }
}
