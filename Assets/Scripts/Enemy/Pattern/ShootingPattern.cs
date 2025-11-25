using UnityEngine;

public abstract class ShootingPattern : IPattern
{
    protected Enemy owner;
    protected EnemyMovement movement;
    protected EnemyTableData enemyData;
    protected PatternSpawner spawner;
    protected PatternExecutor executor;

    public abstract int PatternId { get; }
    public ExecutionTrigger Trigger { get; protected set;}
    public float TriggerValue { get; protected set;} //Interval

    protected float lastExecuteTime;
    protected bool isExecuteOneTime;

    public virtual void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        this.owner = enemy;
        this.movement = movement;
        this.enemyData = enemyData;
        this.spawner = PatternSpawner.Instance;
        this.executor = enemy.GetComponent<PatternExecutor>();

        lastExecuteTime = Time.time;
        isExecuteOneTime = false;
    }

    public virtual bool CanExecute()
    {
        if(owner == null || owner.IsDead)
        {
            return false;
        }

        switch (Trigger)
        {
            case ExecutionTrigger.OnPatternLine:
                return executor.IsPatternLine && !isExecuteOneTime;
            case ExecutionTrigger.OnInterval:
                return Time.time - lastExecuteTime >= TriggerValue;
            case ExecutionTrigger.Immediate:
                return !isExecuteOneTime;
        }

        return false;
    }

    public virtual void Execute()
    {
        Shoot();

        lastExecuteTime = Time.time;

        if(Trigger != ExecutionTrigger.OnInterval)
        {
            isExecuteOneTime = true;
        }
    }

    protected abstract void Shoot();

    public virtual void Reset()
    {
        lastExecuteTime = Time.time;
        isExecuteOneTime = false;
    }

    public void PatternUpdate()
    {
        
    }
}
