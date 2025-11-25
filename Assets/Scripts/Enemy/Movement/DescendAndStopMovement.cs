using UnityEngine;

public class DescendAndStopMovement : IMovement
{
    private Vector3 targetPosition;
    private bool isArrived;
    private float arrivalThreshold = 0.01f;
    private float colliderRadius;

    public void Initialize()
    {
        isArrived = false;
        colliderRadius = 0f;

        Rect offSetBounds = SpawnManager.Instance.OffSetBounds;

        targetPosition = new Vector3(offSetBounds.center.x, offSetBounds.yMin, 0f);
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if(isArrived)
        {
            return Vector3.zero;
        }

        if(colliderRadius == 0f)
        {
            Collider ownerCollider = ownerTransform.GetComponent<Collider>();
            if(ownerCollider != null)
            {
                var extents = ownerCollider.bounds.extents;
                colliderRadius = Mathf.Max(extents.x, extents.y, extents.z);
            }
            else
            {
                colliderRadius = 0.5f;
            }
        }

        var currentPosition = ownerTransform.position;
        var distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

        if(distanceToTarget <= colliderRadius)
        {
            isArrived = true;
            return Vector3.zero;
        }

        return (targetPosition - currentPosition).normalized;
    }

    public void OnPatternLine()
    {
        
    }

    public bool IsCompleted() => isArrived;
}
