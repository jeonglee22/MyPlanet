using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTargetPriority : ScriptableObject,ITargetPrioriy
{
    [SerializeField] private bool isDescending = true; //True:Max, False:Min
    public bool IsDescending => isDescending;
    //protected virtual int GetPriorityId(ITargetable target) => 0; //fixed data id value
    protected virtual float GetPriorityValue(ITargetable target) => 0f;// realtime value
    public ITargetable SelectTarget(IEnumerable<ITargetable> targets)
    {
        var validTargets = new List<(ITargetable target, float priority)>();
        // Debug.Log($"[SelectTarget START] Type: {this.GetType().Name} | isDescending: {isDescending}");


        foreach (var t in targets)
        {
            if (t == null || !t.isAlive) continue;

            validTargets.Add((t,GetPriorityValue(t)));
        }

        // Debug.Log($"[SelectTarget] Valid targets count: {validTargets.Count} | Sorting...");

        validTargets.Sort((a, b) => 
            isDescending ? b.priority.CompareTo(a.priority): a.priority.CompareTo(b.priority));

        var selected = validTargets.Count > 0 ? validTargets[0].target : null;
        // Debug.Log($"[SelectTarget END] Selected: {(selected as MonoBehaviour)?.name}");

        return validTargets.Count > 0 ? validTargets[0].target : null;
    }
}