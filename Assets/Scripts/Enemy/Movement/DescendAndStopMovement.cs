using UnityEngine;

public class DescendAndStopMovement : IMovement
{
    private Vector3 targetPosition;
    private bool isArrived;
    private float colliderRadius;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private float offset = 1f;
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
            SphereCollider sphereCollider = ownerTransform.GetComponent<SphereCollider>();
            if(sphereCollider != null)
            {
                colliderRadius = sphereCollider.radius * ownerTransform.localScale.x;
            }
            else
            {
                colliderRadius = 0.5f;
            }
        }

        Vector3 actualTargetPosition = targetPosition + Vector3.up * colliderRadius;

        var currentPosition = ownerTransform.position;
        var distanceToTarget = Vector3.Distance(currentPosition, actualTargetPosition);

        if(distanceToTarget <= colliderRadius * 0.1f)
        {
            isArrived = true;
            return Vector3.zero;
        }

        Vector3 direction = (actualTargetPosition - currentPosition).normalized;
        ownerTransform.LookAt(ownerTransform.position + direction);

        return direction;
    }

    public void OnPatternLine()
    {
        
    }

    public bool IsCompleted() => isArrived;

    public float GetSpeedMultiplier() => isArrived ? 1f : initialSpeedMultiplier;
}
