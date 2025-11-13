using UnityEngine;

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