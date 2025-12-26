using UnityEngine;

public class DescendAndStopMovement : IMovement
{
    private Vector3 targetPosition;
    private bool isArrived;
    private float colliderRadius;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private float offset = 0.7f;
    private float initialSpeedMultiplier = 2.5f;

    private int enemyType;

    public void Initialize(Enemy owner)
    {
        isArrived = false;
        colliderRadius = 0f;

        Rect offSetBounds = SpawnManager.Instance.OffSetBounds;

        targetPosition = new Vector3(offSetBounds.center.x, offSetBounds.yMin - offset, 0f);

        enemyType = owner.EnemyType;
        if(enemyType <= 2)
        {
            initialSpeedMultiplier = 1f;
        }
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
                colliderRadius = 1.5f;
            }
        }

        var currentPosition = ownerTransform.position;
        var distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

        if(distanceToTarget <= colliderRadius * 0.5f)
        {
            isArrived = true;
            return Vector3.zero;
        }

        Vector3 direction = (targetPosition - currentPosition).normalized;
        ownerTransform.LookAt(ownerTransform.position + direction);

        return direction;
    }

    public void OnPatternLine()
    {
        
    }

    public bool IsCompleted() => isArrived;

    public float GetSpeedMultiplier() => isArrived ? 1f : initialSpeedMultiplier;
}
