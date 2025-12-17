using System.Collections.Generic;
using UnityEngine;

public class SideMovement : IMovement
{
    public bool IsPatternLine => false;
    Rect screenBounds;
    private Vector3 leftPoint;
    private Vector3 rightPoint;
    private Vector3 currentTarget;
    private bool isInitialized = false;
    private bool hasReachedFistTarget = false;
    private float yPos;

    private int enemyType;

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        Vector3 currentPos = ownerTransform.position;

        if (!isInitialized)
        {
            float colliderHalfExtent = 0f;
            var collider = ownerTransform.GetComponent<Collider>();
            if(collider != null)
            {
                colliderHalfExtent = collider.bounds.extents.x;
            }

            leftPoint = new Vector3(screenBounds.xMin + colliderHalfExtent, yPos, 0f);
            rightPoint = new Vector3(screenBounds.xMax - colliderHalfExtent, yPos, 0f);

            currentPos = ownerTransform.position;

            float distToLeft = Vector3.Distance(currentPos, leftPoint);
            float distToRight = Vector3.Distance(currentPos, rightPoint);

            currentTarget = distToLeft < distToRight ? leftPoint : rightPoint;

            isInitialized = true;
        }

        float distanceToTarget = Vector3.Distance(currentPos, currentTarget);
        if (distanceToTarget < 0.05f)
        {
            if(!hasReachedFistTarget)
            {
                hasReachedFistTarget = true;
            }
            currentTarget = currentTarget == leftPoint ? rightPoint : leftPoint;
        }

        Vector3 direction = (currentTarget - currentPos).normalized;

        ownerTransform.LookAt(ownerTransform.position + direction);

        return direction;
    }

    public void Initialize(Enemy owner)
    {
        screenBounds = SpawnManager.Instance.ScreenBounds;
        
        yPos = SpawnManager.Instance.Spawners[0].transform.position.y;

        leftPoint = new Vector3(screenBounds.xMin, yPos, 0f);
        rightPoint = new Vector3(screenBounds.xMax, yPos, 0f);

        currentTarget = Vector3.zero;

        isInitialized = false;
        hasReachedFistTarget = false;

        enemyType = owner.EnemyType;
    }

    public bool IsCompleted() => hasReachedFistTarget;

    public void OnPatternLine()
    {
        
    }

    public float GetSpeedMultiplier() => hasReachedFistTarget ? 1f : 2.5f;
}
