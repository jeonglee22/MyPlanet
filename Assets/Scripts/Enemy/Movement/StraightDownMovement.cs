using UnityEngine;

public class StraightDownMovement : IMovement
{
    public void Initialize()
    {
        
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        return baseDirection;
    }

    public void OnPatternLine()
    {
        
    }

    public bool IsCompleted() => false;
}
