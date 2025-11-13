using UnityEngine;

[CreateAssetMenu(menuName = "TargetPriority/LowestMaxHP")]
public class LowestMaxHpPrioritySO : BaseTargetPriority
{
    protected override float GetPriorityValue(ITargetable target) => target.maxHp;
}