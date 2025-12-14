using UnityEngine;

public class ChaseMovement : IMovement
{
    private bool isPatternLine = false;

    public bool IsPatternLine => isPatternLine;

    private int enemyType;

    public void Initialize(Enemy owner)
    {
        isPatternLine = false;
        enemyType = owner.EnemyType;
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
        if(enemyType <= 1)
        {
            return;
        }
        isPatternLine = true;
    }

    public bool IsCompleted() => false;

    public float GetSpeedMultiplier() => 1f;
}
