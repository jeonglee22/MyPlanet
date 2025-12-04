using System.Threading;
using Cysharp.Threading.Tasks;
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
    public virtual bool RequireAsync { get; protected set;} = false;

    protected PatternData patternData;

    protected float lastExecuteTime;
    protected bool isExecuteOneTime;

    protected Transform target;
    protected Planet planet;

    public virtual void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        owner = enemy;
        this.movement = movement;
        this.enemyData = enemyData;
        spawner = PatternSpawner.Instance;
        executor = enemy.GetComponent<PatternExecutor>();

        patternData = enemy.CurrentPatternData;

        lastExecuteTime = Time.time;
        isExecuteOneTime = false;

        if(target == null)
        {
            planet = GameObject.FindGameObjectWithTag(TagName.Planet).GetComponent<Planet>();
            target = planet.transform;
        }
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

    public virtual UniTask ExecuteAsync(CancellationToken token)
    {
        Execute();
        return UniTask.CompletedTask;
    }

    protected abstract void Shoot();

    public virtual void Reset()
    {
        lastExecuteTime = Time.time;
        isExecuteOneTime = false;
    }

    public virtual void PatternUpdate()
    {
        
    }

    public PatternData GetPatternData() => patternData;
}
