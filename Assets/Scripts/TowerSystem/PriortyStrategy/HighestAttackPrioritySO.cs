using UnityEngine;

[CreateAssetMenu(menuName = "TargetPriority/HighestDefense")]
public class HighestAttackPrioritySO : BaseTargetPriority
{
    protected override float GetPriorityValue(ITargetable target) => target.atk;
}