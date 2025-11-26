using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PatternExecutor : MonoBehaviour
{
    private Enemy owner;
    private List<IPattern> patterns = new List<IPattern>();
    
    private Dictionary<IPattern, float> patternCooldowns = new Dictionary<IPattern, float>();
    private Dictionary<IPattern, int> patternRepeatExecutions = new Dictionary<IPattern, int>();
    private Dictionary<IPattern, float> patternWeights = new Dictionary<IPattern, float>();

    public bool IsPatternLine { get; set; } = false;

    private bool isExecutePattern = false;
    private CancellationTokenSource patternCts;

    private float patternTimer = 0f;
    private float patternInterval = 3f;

    private void OnDestroy()
    {
        Cancel();
    }

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
        patterns.Clear();
        patternCooldowns.Clear();
        patternRepeatExecutions.Clear();
        patternWeights.Clear();
        IsPatternLine = false;


        isExecutePattern = false;

        Cancel();
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

        Cancel();
    }

    public void ClearPatterns()
    {
        Cancel();

        patterns.Clear();
        patternCooldowns.Clear();
        patternRepeatExecutions.Clear();
        patternWeights.Clear();

        isExecutePattern = false;
    }

    private void Update()
    {
        if(owner == null || owner.IsDead)
        {
            Cancel();
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

        if(isExecutePattern)
        {
            return;
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
            if(selectedPattern != null)
            {
                ExecutePatternAsync(selectedPattern, patternCts.Token).Forget();
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

    private async UniTaskVoid ExecutePatternAsync(IPattern pattern, CancellationToken token)
    {
        var patternData = pattern.GetPatternData();
        if(patternData == null)
        {
            return;
        }

        isExecutePattern = true;

        try
        {
            for(int i = 0; i < patternRepeatExecutions[pattern]; i++)
            {
                token.ThrowIfCancellationRequested();

                pattern.Execute();

                if(i < patternRepeatExecutions[pattern] - 1 && patternData.RepeatDelay > 0f)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(patternData.RepeatDelay), cancellationToken: token);
                }
            }

            if(patternData.PatternDelay > 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(patternData.PatternDelay), cancellationToken: token);
            }

            patternCooldowns[pattern] = patternData.Cooltime;
        }
        catch(System.OperationCanceledException)
        {
            
        }
        finally
        {
            isExecutePattern = false;
        }
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

    public void Cancel()
    {
        patternCts?.Cancel();
        patternCts?.Dispose();
        patternCts = new CancellationTokenSource();
    }
}
