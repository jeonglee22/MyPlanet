using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTargetPriority : ITargetPrioriy
{
    protected abstract float GetPriorityValue(ITargetable target);
    public ITargetable SelectTarget(IEnumerable<ITargetable> targets)
    {
        var validTargets = new List<(ITargetable target, float priority)>();

        foreach(var t in targets)
        {
            if (t == null || !t.isAlive) continue;

            float value = GetPriorityValue(t);
            validTargets.Add((t, value));
        }
        validTargets.Sort((a, b) => b.priority.CompareTo(a.priority));
        return validTargets.Count > 0 ? validTargets[0].target : null;
    }
}
