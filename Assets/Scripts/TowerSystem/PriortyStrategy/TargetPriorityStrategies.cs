using UnityEngine;

[CreateAssetMenu(menuName = "TargetPriority/HighestHP")]
public class HighestHpPrioritySO : BaseTargetPriority
{//ID 받아오는 것으로 변경 필요
    protected override float GetPriorityValue(ITargetable target) => target.maxHp;
}

[CreateAssetMenu(menuName = "TargetPriority/HighestAttack")]
public class HighestAttackPrioritySO : BaseTargetPriority
{
    protected override float GetPriorityValue(ITargetable target) => target.atk;
}

[CreateAssetMenu(menuName = "TargetPriority/HighestDefense")]
public class HighestDefensePrioritySO : BaseTargetPriority
{
    protected override float GetPriorityValue(ITargetable target) => target.def;
}
