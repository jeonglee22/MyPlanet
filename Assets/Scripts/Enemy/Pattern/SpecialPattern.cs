using UnityEngine;

public abstract class SpecialPattern : IPattern
{
    protected Enemy owner;
    protected EnemyMovement movement;
    protected EnemyTableData enemyData;
    protected PatternExecutor executor;

    public abstract int PatternId { get; }
    public ExecutionTrigger Trigger { get; protected set;}
    public float TriggerValue { get; protected set;}

    protected PatternData patternData;

    protected bool isExecuteOneTime;

    public virtual void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data)
    {
        this.owner = enemy;
        this.movement = movement;
        this.enemyData = data;
        this.executor = enemy.GetComponent<PatternExecutor>();

        patternData = enemy.CurrentPatternData;

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
            case ExecutionTrigger.Immediate:
                return !isExecuteOneTime;
        }

        return false;
    }

    public abstract void Execute();

    public abstract void PatternUpdate();

    public virtual void Reset()
    {
        isExecuteOneTime = false;
    }

    public PatternData GetPatternData() => patternData;
}
