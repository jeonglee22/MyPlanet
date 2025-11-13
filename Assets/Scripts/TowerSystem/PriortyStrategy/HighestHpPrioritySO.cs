using UnityEngine;

[CreateAssetMenu(menuName = "TargetPriority/HighestHP")]
public class HighestHpPrioritySO : BaseTargetPriority
{
    protected override float GetPriorityValue(ITargetable target) => target.maxHp;
}