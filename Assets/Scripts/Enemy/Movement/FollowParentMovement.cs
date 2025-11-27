using UnityEngine;

public class FollowParentMovement : IMovement
{
    private Transform parentTransform;
    private Vector3 localOffset;
    private bool isInitialized = false;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    public void Initialize()
    {
        if(!isInitialized)
        {
            parentTransform = null;
            localOffset = Vector3.zero;
        }
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if (!isInitialized || parentTransform == null)
        {
            return baseDirection;
        }

        Vector3 targetPosition = parentTransform.position + localOffset;
        Vector3 direction = targetPosition - ownerTransform.position;

        if(direction.sqrMagnitude < 0.01f)
        {
            return Vector3.zero;
        }

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
}
