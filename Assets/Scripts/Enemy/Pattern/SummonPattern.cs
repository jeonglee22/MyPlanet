using UnityEngine;

public abstract class SummonPattern : IPattern
{
    protected Enemy owner;
    protected EnemyMovement movement;
    protected EnemyTableData enemyData;
    protected PatternExecutor executor;

    protected float lastExecuteTime;
    protected bool isExecuteOneTime;
    
    public abstract int PatternId { get; }
    public ExecutionTrigger Trigger { get; protected set;}
    public float TriggetValue { get; protected set;} //Interval

    public void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data)
    {
        owner = enemy;
        this.movement = movement;
        enemyData = data;
        executor = enemy.GetComponent<PatternExecutor>();

        lastExecuteTime = Time.time;
        isExecuteOneTime = false;
    }

    public bool CanExecute()
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
                return Time.time - lastExecuteTime >= TriggetValue;
            case ExecutionTrigger.Immediate:
                return !isExecuteOneTime;
        }

        return false;
    }

    public void Execute()
    {
        Summon();

        lastExecuteTime = Time.time;

        if(Trigger != ExecutionTrigger.OnInterval)
        {
            isExecuteOneTime = true;
        }
    }

    public void PatternUpdate()
    {
        
    }

    public void Reset()
    {
        lastExecuteTime = Time.time;
        isExecuteOneTime = false;
    }

    protected abstract void Summon();
}
