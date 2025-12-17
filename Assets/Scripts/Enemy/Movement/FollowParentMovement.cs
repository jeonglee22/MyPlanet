using UnityEngine;

public class FollowParentMovement : IMovement
{
    private Transform parentTransform;
    private Vector3 localOffset;
    private bool isInitialized = false;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private int enemyType;

    public void Initialize(Enemy owner)
    {
        if(!isInitialized)
        {
            parentTransform = null;
            localOffset = Vector3.zero;
        }

        enemyType = owner.EnemyType;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if (!isInitialized || parentTransform == null)
        {
            return baseDirection;
        }

        Vector3 targetPosition = parentTransform.position + localOffset;
        Vector3 direction = targetPosition - ownerTransform.position;
        ownerTransform.LookAt(ownerTransform.position + direction);

        return direction;
    }

    public void OnPatternLine()
    {
        
    }

    public void SetParent(Transform parent, Vector3 offset)
    {
        parentTransform = parent;
        localOffset = offset;
        isInitialized = true;
    }

    public bool IsCompleted() => false;

    public float GetSpeedMultiplier() => 1f;
}
