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

    private List<IPattern> availablePatterns = new List<IPattern>();
    private List<float> weights = new List<float>();

    public bool IsPatternLine { get; set; } = false;

    private bool isExecutePattern = false;
    private CancellationTokenSource patternCts;

    private bool canExecutePattern = false;

    private IPattern currentPattern;
    private int currentRepeatIndex;
    private int currentRepeatCount;
    private float repeatDelayTimer;
    private float patternDelayTimer;
    private PatternData currentPatternData;
    private bool isWaitingRepeatDelay;
    private bool isWaitingPatternDelay;

    private void OnDisable()
    {
        Cancel();
    }

    private void OnDestroy()
    {
        Cancel();
    }

    private void Start()
    {
        if(owner.Data.EnemyType != 3 && owner.Data.EnemyType != 4)
        {
            canExecutePattern = true;
        }
    }

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
        patterns.Clear();
        patternCooldowns.Clear();
        patternRepeatExecutions.Clear();
        patternWeights.Clear();
        availablePatterns.Clear();
        weights.Clear();
        IsPatternLine = false;

        isExecutePattern = false;

        currentPattern = null;
        currentRepeatIndex = 0;
        isWaitingPatternDelay = false;
        isWaitingRepeatDelay = false;

        Cancel();
    }

    public void OnBossReady() => canExecutePattern = true;

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
        availablePatterns.Clear();
        weights.Clear();

        isExecutePattern = false;
        currentPattern = null;
        isWaitingPatternDelay = false;
        isWaitingRepeatDelay = false;
    }

    private void Update()
    {
        if(owner == null || owner.IsDead)
        {
            Cancel();
            return;
        }

        if(owner.Data.EnemyType == 4 && Variables.MiddleBossEnemy != null && !Variables.MiddleBossEnemy.IsDead)
        {
            return;
        }

        if(!canExecutePattern)
        {
            return;
        }

        if(isExecutePattern)
        {
            HandlePatternExecution();
            return;
        }

        bool hasHealthPercentagePattern = false;
        IPattern healthPercentagePattern = null;

        bool hasOrbitReachedPattern = false;
        IPattern orbitReachedPattern = null;

        availablePatterns.Clear();
        weights.Clear();

        float deltaTime = Time.deltaTime;

        for(int i = 0; i < patterns.Count; i++)
        {
            var pattern = patterns[i];

            pattern.PatternUpdate();

            if(pattern.Trigger == ExecutionTrigger.Immediate && !isExecutePattern)
            {
                ExecutePatternAsync(pattern, patternCts.Token).Forget();
                continue;
            }

            if(patternCooldowns.TryGetValue(pattern, out float cooldown))
            {
                if(cooldown > 0f)
                {
                    patternCooldowns[pattern] -= deltaTime;
                    continue;
                }
            }

            if(!pattern.CanExecute())
            {
                continue;
            }

            if(pattern.Trigger == ExecutionTrigger.OnOrbitReached)
            {
                hasOrbitReachedPattern = true;
                orbitReachedPattern = pattern;
                break;
            }

            if(pattern.Trigger == ExecutionTrigger.OnHealthPercentage)
            {
                hasHealthPercentagePattern = true;
                healthPercentagePattern = pattern;
                break;
            }

            availablePatterns.Add(pattern);
            if(patternWeights.TryGetValue(pattern, out float weight))
            {
                weights.Add(weight);
            }
        }

        if(isExecutePattern)
        {
            return;
        }

        IPattern selectedPattern = hasOrbitReachedPattern ? orbitReachedPattern : hasHealthPercentagePattern ? healthPercentagePattern : SelectPatternWeight(availablePatterns, weights);
        if(selectedPattern != null)
        {
            ExecutePatternAsync(selectedPattern, patternCts.Token).Forget();
        }
    }

    private void HandlePatternExecution()
    {
        float deltaTIme = Time.deltaTime;

        if(isWaitingRepeatDelay)
        {
            repeatDelayTimer -= deltaTIme;
            if(repeatDelayTimer <= 0f)
            {
                isWaitingRepeatDelay = false;
                ExecutePatternStep();
            }
            return;
        }
        
        if(isWaitingPatternDelay)
        {
            patternDelayTimer -= deltaTIme;
            if(patternDelayTimer <= 0f)
            {
                isWaitingPatternDelay = false;
                FinishPattern();
            }
            return;
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
        if(!patternRepeatExecutions.ContainsKey(pattern))
        {
            return;
        }

        var patternData = pattern.GetPatternData();
        if(patternData == null)
        {
            return;
        }

        currentPattern = pattern;
        currentPatternData = patternData;
        currentRepeatCount = patternRepeatExecutions[pattern];
        currentRepeatIndex = 0;
        isExecutePattern = true;

        if(pattern.RequireAsync)
        {
            ExecutePatternAsync(pattern, patternCts.Token).Forget();
        }
        else
        {
            ExecutePatternStep();
        }
    }

    private async UniTaskVoid ExecutePatternAsync(IPattern pattern, CancellationToken token)
    {
        if(!patternRepeatExecutions.ContainsKey(pattern))
        {
            return;
        }

        var patternData = pattern.GetPatternData();
        if(patternData == null)
        {
            return;
        }

        int repeatCount = patternRepeatExecutions[pattern];
        float repeatDelay = patternData.RepeatDelay;
        float patternDelay = patternData.PatternDelay;
        float cooltime = patternData.Cooltime;

        isExecutePattern = true;

        try
        {
            for(int i = 0; i < repeatCount; i++)
            {
                token.ThrowIfCancellationRequested();

                if(pattern.RequireAsync)
                {
                    await pattern.ExecuteAsync(token);
                }
                else
                {
                    pattern.Execute();
                }

                if(i < repeatCount - 1 && repeatDelay > 0f)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(repeatDelay), cancellationToken: token);
                }
            }

            if(patternDelay > 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(patternDelay), cancellationToken: token);
            }

            if(patternCooldowns.ContainsKey(pattern))
            {
                patternCooldowns[pattern] = cooltime;
            }
        }
        catch(System.OperationCanceledException)
        {
            
        }
        finally
        {
            isExecutePattern = false;
        }
    }

    private void ExecutePatternStep()
    {
        if(currentPattern == null || currentPatternData == null)
        {
            isExecutePattern = false;
            return;
        }

        currentPattern.Execute();
        currentRepeatIndex++;

        if(currentRepeatIndex >= currentRepeatCount)
        {
            if(currentPatternData.PatternDelay > 0f)
            {
                isWaitingPatternDelay = true;
                patternDelayTimer = currentPatternData.PatternDelay;
            }
            else
            {
                FinishPattern();
            }
        }
        else
        {
            if(currentPatternData.RepeatDelay > 0f)
            {
                isWaitingRepeatDelay = true;
                repeatDelayTimer = currentPatternData.RepeatDelay;
            }
            else
            {
                ExecutePatternStep();
            }
        }
    }

    private void FinishPattern()
    {
        if(currentPattern != null && patternCooldowns.ContainsKey(currentPattern))
        {
            patternCooldowns[currentPattern] = currentPatternData.Cooltime;
        }

        currentPattern = null;
        currentPatternData = null;
        isExecutePattern = false;
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
