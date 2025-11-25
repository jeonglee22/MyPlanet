using UnityEngine;

public class ChaseMovement : IMovement
{
    private bool isPatternLine = false;

    public void Initialize()
    {
        isPatternLine = false;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if(!isPatternLine || target == null)
        {
            return baseDirection;
        }

        Vector3 chaseDirection = (target.position - ownerTransform.position).normalized;
        ownerTransform.LookAt(ownerTransform.position + chaseDirection);

        return chaseDirection;
    }

    public void OnPatternLine()
    {
        isPatternLine = true;
    }

    public bool IsCompleted() => false;
}
