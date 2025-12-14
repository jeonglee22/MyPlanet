using UnityEngine;

public class HomingMovement : IMovement
{
    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;
    private bool isDirectionSet = false;
    private Vector3 currentMoveDirection;

    private int enemyType;

    public void Initialize(Enemy owner)
    {
        isDirectionSet = false;
        enemyType = owner.EnemyType;

        if(enemyType <= 1)
        {
            isPatternLine = true;
        }
        else
        {
            isPatternLine = false;
        }
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
        if(enemyType <= 1)
        {
            return;
        }
        isPatternLine = true;
    }

    public bool IsCompleted() => isDirectionSet;

    public float GetSpeedMultiplier() => 1f;
}
