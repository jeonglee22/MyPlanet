using System.Collections.Generic;
using UnityEngine;

public class PatternExecutor : MonoBehaviour
{
    private Enemy owner;
    private List<IPattern> patterns = new List<IPattern>();
    
    private Dictionary<IPattern, float> patternCooldowns = new Dictionary<IPattern, float>();
    private Dictionary<IPattern, int> patternRepeatExecutions = new Dictionary<IPattern, int>();
    private Dictionary<IPattern, float> patternWeights = new Dictionary<IPattern, float>();

    private float patternTimer = 0f;
    private float patternInterval = 3f;

    public bool IsPatternLine { get; set; } = false;

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
        patterns.Clear();
        patternCooldowns.Clear();
        patternRepeatExecutions.Clear();
        patternWeights.Clear();
        patternTimer = 0f;
        IsPatternLine = false;
    }

    public void AddPattern(IPattern pattern)
    {
        if(pattern == null)
        {
            return;
        }

        patterns.Add(pattern);
        patternCooldowns[pattern] = 0f;
        patternRepeatExecutions[pattern] = owner.CurrentPatternData.PatternValue;

        var patternData = pattern.GetPatternData();
        if(patternData != null)
        {
            patternWeights[pattern] = patternData.Weight;
        }
    }

    public void RemovePattern(IPattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        patterns.Remove(pattern);
        patternCooldowns.Remove(pattern);
        patternRepeatExecutions.Remove(pattern);
        patternWeights.Remove(pattern);
    }

    public void ClearPatterns()
    {
        patterns.Clear();
        patternCooldowns.Clear();
        patternRepeatExecutions.Clear();
    }

    private void Update()
    {
        if(owner == null || owner.IsDead)
        {
            return;
        }

        foreach (var pattern in patterns)
        {
            pattern.PatternUpdate();

            if (patternCooldowns.ContainsKey(pattern) && patternCooldowns[pattern] > 0f)
            {
                patternCooldowns[pattern] -= Time.deltaTime;
            }

            if(pattern.Trigger == ExecutionTrigger.Immediate)
            {
                pattern.Execute();
            }
        }

        //Can execute patterns
        List<IPattern> availablePatterns = new List<IPattern>();
        List<float> weights = new List<float>();

        foreach(var pattern in patterns)
        {
            if(patternCooldowns[pattern] <= 0f && pattern.CanExecute())
            {
                availablePatterns.Add(pattern);

                weights.Add(patternWeights[pattern]);
            }
        }

        patternTimer += Time.deltaTime;
        if(patternTimer > patternInterval && availablePatterns.Count > 0)
        {
            IPattern selectedPattern = SelectPatternWeight(availablePatterns, weights);
            if(GetPatternData(selectedPattern).Pattern_Id == (int)PatternIds.TitanEleteMeteorClusterSummon)
            {
                Debug.Log("Titan");
            }
            if(selectedPattern != null)
            {
                ExecutePattern(selectedPattern);
            }
            patternTimer = 0f;
        }
    }

    public void ResetAllPatterns()
    {
        foreach (var pattern in patterns)
        {
            pattern.Reset();
        }
    }

    public void OnPatternLine()
    {
        IsPatternLine = true;
    }

    private void ExecutePattern(IPattern pattern)
    {
        var patternData = pattern.GetPatternData();
        if(patternData == null)
        {
            return;
        }

        for(int i = 0; i < patternRepeatExecutions[pattern]; i++)
        {
            pattern.Execute();
        }

        patternCooldowns[pattern] = patternData.Cooltime;
    }

    private PatternData GetPatternData(IPattern pattern)
    {
        return pattern.GetPatternData();
    }

    private IPattern SelectPatternWeight(List<IPattern> patterns, List<float> weights)
    {
        if(patterns.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach(float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float comparisonWeight = 0f;

        for(int i = 0; i < patterns.Count; i++)
        {
            comparisonWeight += weights[i];
            if(randomValue <= comparisonWeight)
            {
                return patterns[i];
            }
        }

        return patterns[patterns.Count - 1];
    }
}
