using UnityEngine;

public abstract class ShootingPattern : EnemyPattern
{
    protected PatternSpawner spawner;

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData, ExecutionTrigger trigger = ExecutionTrigger.None, float interval = 0f)
    {
        base.Initialize(enemy, movement, enemyData, trigger, interval);
        spawner = PatternSpawner.Instance;
    }

    public override void Execute()
    {
        if(spawner == null)
        {
            return;
        }

        Shoot();
    }

    protected abstract void Shoot();
}
