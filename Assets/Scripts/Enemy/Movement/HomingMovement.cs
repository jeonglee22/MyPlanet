using UnityEngine;

public class HomingMovement : IMovement
{
    private bool isPatternLine = false;
    private bool isDirectionSet = false;
    private Vector3 currentMoveDirection;

    public void Initialize()
    {
        isPatternLine = false;
        isDirectionSet = false;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if (!isPatternLine)
        {
            return baseDirection;
        }

        if(!isDirectionSet && target != null)
        {
            currentMoveDirection = (target.position - ownerTransform.position).normalized;
            ownerTransform.LookAt(ownerTransform.position + currentMoveDirection);
            isDirectionSet = true;
        }

        return currentMoveDirection;
    }

    public void OnPatternLine()
    {
        isPatternLine = true;
    }
}
