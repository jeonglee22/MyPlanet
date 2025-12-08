using UnityEngine;

public class StraightDownMovement : IMovement
{
    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private int enemyType;

    public void Initialize(int enemyType)
    {
        this.enemyType = enemyType;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        return baseDirection;
    }

    public void OnPatternLine()
    {
        
    }

    public bool IsCompleted() => false;

    public float GetSpeedMultiplier() => 1f;
}
