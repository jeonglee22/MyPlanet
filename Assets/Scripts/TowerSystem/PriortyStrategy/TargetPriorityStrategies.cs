using UnityEngine;

[CreateAssetMenu(menuName = "TargetPriority/HighestHP")]
public class HighestHpPrioritySO : BaseTargetPriority
{
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

[CreateAssetMenu(menuName = "TargetPriority/ClosestDistance")]
public class ClosestDistancePrioritySO : BaseTargetPriority
{
    private Transform towerTransform; //runtime in TowerTargetingSystem

    public void Initialize(Transform transform)
    {
        towerTransform = transform;
    }

    protected override float GetPriorityValue(ITargetable target)
    {
        return -Vector3.Distance(towerTransform.position, target.position);
    }
}