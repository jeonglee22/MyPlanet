using UnityEngine;

public class ChaseMovement : IMovement
{
    private bool isPatternLine = false;

    public bool IsPatternLine => isPatternLine;

    private int enemyType;

    private Vector3 lastDirection;
    private const float DIRECTION_THRESHOLD = 0.01f;

    public void Initialize(Enemy owner)
    {
        isPatternLine = false;
        enemyType = owner.EnemyType;

        if(enemyType <= 2)
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
        if(!isPatternLine || target == null)
        {
            return baseDirection;
        }

        Vector3 chaseDirection = (target.position - ownerTransform.position).normalized;

        if((chaseDirection - lastDirection).sqrMagnitude > DIRECTION_THRESHOLD)
        {
            ownerTransform.LookAt(ownerTransform.position + chaseDirection);
            lastDirection = chaseDirection;
        }

        return chaseDirection;
    }

    public void OnPatternLine()
    {
        if(enemyType <= 1)
        {
            return;
        }
        isPatternLine = true;
    }

    public bool IsCompleted() => false;

    public float GetSpeedMultiplier() => 1f;
}
