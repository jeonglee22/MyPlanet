using System.Collections.Generic;
using UnityEngine;

public class PatternExecutor : MonoBehaviour
{
    private Enemy owner;
    private List<IPattern> patterns = new List<IPattern>();

    public bool IsPatternLine { get; set; } = false;

    public void Initialize(Enemy enemy)
    {
        owner = enemy;
        patterns.Clear();
        IsPatternLine = false;
    }

    public void AddPattern(IPattern pattern)
    {
        if(pattern == null)
        {
            return;
        }

        patterns.Add(pattern);
    }

    public void RemovePattern(IPattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        patterns.Remove(pattern);
    }

    public void ClearPatterns()
    {
        patterns.Clear();
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

            if (pattern.CanExecute())
            {
                pattern.Execute();
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
}
