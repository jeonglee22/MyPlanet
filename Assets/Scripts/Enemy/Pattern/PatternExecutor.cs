using System.Collections.Generic;
using UnityEngine;

public class PatternExecutor : MonoBehaviour
{
    private Enemy owner;
    private List<IPattern> patterns = new List<IPattern>();
    
    private Dictionary<IPattern, float> patternCooldowns = new Dictionary<IPattern, float>();
    private Dictionary<IPattern, int> patternRemainingExecutions = new Dictionary<IPattern, int>();

    public bool IsPatternLine { get; set; } = false;

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
        patterns.Clear();
        patternCooldowns.Clear();
        patternRemainingExecutions.Clear();
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
        patternRemainingExecutions[pattern] = 0;
    }

    public void RemovePattern(IPattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        patterns.Remove(pattern);
        patternCooldowns.Remove(pattern);
        patternRemainingExecutions.Remove(pattern);
    }

    public void ClearPatterns()
    {
        patterns.Clear();
        patternCooldowns.Clear();
        patternRemainingExecutions.Clear();
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
        }

        //Can execute patterns
        List<IPattern> availablePatterns = new List<IPattern>();
        List<float> weights = new List<float>();

        foreach(var pattern in patterns)
        {
            if(patternCooldowns[pattern] <= 0f && pattern.CanExecute())
            {
                availablePatterns.Add(pattern);

                var patternData = pattern.GetPatternData();
                if(patternData != null)
                {
                    weights.Add(patternData.Weight);
                }
                else
                {
                    weights.Add(1f);
                }
            }
        }

        if(availablePatterns.Count > 0)
        {
            IPattern selectedPattern = SelectPatternWeight(availablePatterns, weights);
            if(selectedPattern != null)
            {
                ExecutePattern(selectedPattern);
            }
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

        patternRemainingExecutions[pattern] = patternData.PatternValue;

        pattern.Execute();
        patternRemainingExecutions[pattern]--; //first execution

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
