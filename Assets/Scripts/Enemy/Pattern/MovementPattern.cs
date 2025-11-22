using UnityEngine;

public abstract class MovementPattern : EnemyPattern
{
    protected float originalSpeed;

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData, ExecutionTrigger trigger, float interval = 0f)
    {
        base.Initialize(enemy, movement, enemyData, trigger, interval);
        originalSpeed = movement.moveSpeed;
    }

    public override void Execute()
    {
        ChangeMovement();
    }

    protected abstract void ChangeMovement();
}
