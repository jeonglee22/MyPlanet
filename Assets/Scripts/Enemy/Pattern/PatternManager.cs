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
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        patternDict = new Dictionary<int, Func<IPattern>>();

        //patternDict.Add((int)PatternIds.MeteorCluster, () => new MeteorClusterPattern());
        patternDict.Add((int)PatternIds.SimpleShot, () => new SimpleShotPattern());
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
