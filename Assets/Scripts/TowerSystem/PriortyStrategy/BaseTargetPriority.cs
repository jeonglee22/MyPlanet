using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTargetPriority : ScriptableObject,ITargetPrioriy
{
    [SerializeField] private bool isDescending = true; //True:Max, False:Min
    //protected virtual int GetPriorityId(ITargetable target) => 0; //fixed data id value
    protected virtual float GetPriorityValue(ITargetable target) => 0f;// realtime value
    public ITargetable SelectTarget(IEnumerable<ITargetable> targets)
    {
        var validTargets = new List<(ITargetable target, float priority)>();

        foreach(var t in targets)
        {
            if (t == null || !t.isAlive) continue;

            validTargets.Add((t,GetPriorityValue(t)));
        }
        
        validTargets.Sort((a, b) => 
            isDescending ? b.priority.CompareTo(a.priority): a.priority.CompareTo(b.priority));
        return validTargets.Count > 0 ? validTargets[0].target : null;
    }
}
