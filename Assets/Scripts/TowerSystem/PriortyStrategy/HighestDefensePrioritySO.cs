using UnityEngine;

[CreateAssetMenu(menuName = "TargetPriority/HighestDefense")]
public class HighestDefensePrioritySO : BaseTargetPriority
{
    protected override float GetPriorityValue(ITargetable target) => target.def;
}