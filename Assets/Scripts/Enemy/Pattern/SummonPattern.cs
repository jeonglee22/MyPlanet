using UnityEngine;

public abstract class SummonPattern : IPattern
{
    protected Enemy owner;
    protected EnemyMovement movement;
    protected EnemyTableData enemyData;
    protected PatternExecutor executor;
    
    public abstract int PatternId { get; }
    public ExecutionTrigger Trigger { get; protected set;}
    public float TriggerValue { get; protected set;} //Interval

    protected PatternData patternData;
    protected MinionSpawnData summonData;
    protected EnemyTableData summonEnemyData;

    protected float lastExecuteTime;
    protected bool isExecuteOneTime;

    public void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data)
    {
        owner = enemy;
        this.movement = movement;
        enemyData = data;

        patternData = owner.CurrentPatternData;
        if(patternData != null)
        {
            summonData = DataTableManager.MinionSpawnTable.Get(patternData.Pattern_Id);
            if(summonData != null)
            {
                summonEnemyData = DataTableManager.EnemyTable.Get(summonData.Enemy_Id);
            }
        }

        lastExecuteTime = Time.time;
        isExecuteOneTime = false;

        TriggerValue = patternData.PatternValue;
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
                return Time.time - lastExecuteTime >= TriggerValue;
            case ExecutionTrigger.Immediate:
                return !isExecuteOneTime;
        }

        return false;
    }

    public virtual void Execute()
    {
        if(owner?.Spawner == null)
        {
            return;
        }

        Summon();

        lastExecuteTime = Time.time;
    }

    public virtual void PatternUpdate()
    {
        
    }

    public void Reset()
    {
        lastExecuteTime = Time.time;
        isExecuteOneTime = false;
    }

    protected abstract void Summon();

    public PatternData GetPatternData() => patternData;
}
