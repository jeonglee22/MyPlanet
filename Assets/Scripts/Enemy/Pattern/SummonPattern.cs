using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
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
    public bool RequireAsync { get; protected set;} = false;

    protected PatternData patternData;
    protected MinionSpawnData summonData;
    protected EnemyTableData summonEnemyData;

    protected float lastExecuteTime;
    protected bool isExecuteOneTime;

    protected float HEALTHPERCENTAGE_THRESHOLD = 0.3f;

    public void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data)
    {
        owner = enemy;
        this.movement = movement;
        enemyData = data;

        patternData = owner.CurrentPatternData;
        if(patternData != null)
        {
            summonData = DataTableManager.MinionSpawnTable.Get(patternData.MinionSpawn_Id);
            if(summonData != null)
            {
                summonEnemyData = DataTableManager.EnemyTable.Get(summonData.Enemy_Id);
            }
        }

        lastExecuteTime = Time.time;
        isExecuteOneTime = false;

        TriggerValue = patternData.Cooltime;
    }

    public bool CanExecute()
    {
        if(owner == null || owner.IsDead)
        {
            return false;
        }

        switch (Trigger)
        {
            case ExecutionTrigger.OnHealthPercentage:
                float healthPercentage = owner.Health / owner.MaxHealth;
                return healthPercentage <= HEALTHPERCENTAGE_THRESHOLD && !isExecuteOneTime;
        }

        return true;
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

    public virtual UniTask ExecuteAsync(CancellationToken token)
    {
        Execute();
        return UniTask.CompletedTask;
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

    protected EnemySpawner GetRandomSpawner()
    {
        int index = UnityEngine.Random.Range(1, SpawnManager.Instance.Spawners.Count - 1);
        return SpawnManager.Instance.Spawners[index];
    }
}
