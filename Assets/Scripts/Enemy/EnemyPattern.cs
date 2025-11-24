using System.Threading;
using UnityEngine;

public abstract class EnemyPattern : MonoBehaviour
{
    protected Enemy owner;
    protected EnemyMovement movement;
    protected EnemyTableData data;
    protected CancellationTokenSource cts;

    //pattern test
    protected ExecutionTrigger executionTrigger;
    protected float interval;
    private float lastExecuteTime;
    private bool hasExecuted; //one time execute check

    public virtual void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData, ExecutionTrigger trigger, float interval = 0f)
    {
        owner = enemy;
        this.movement = movement;
        data = enemyData;

        executionTrigger = trigger;
        this.interval = interval;
        lastExecuteTime = Time.time;
        hasExecuted = false;
    }

    public virtual float CalculateDamage(float damage)
    {
        return damage;
    }

    protected virtual void Update()
    {
        if(executionTrigger == ExecutionTrigger.OnInterval)
        {
            if(Time.time - lastExecuteTime >= interval)
            {
                Execute();
                lastExecuteTime = Time.time;
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TagName.Enemy))
        {
            return;
        }
        
        IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.OnDamage(owner.atk);
            owner.Cancel();
        }

        if(executionTrigger == ExecutionTrigger.OnPatternLine && !hasExecuted && other.CompareTag("PatternLine"))
        {
            Execute();
            hasExecuted = true;

            owner?.OnPatternLineTrigger();
        }
    }

    protected virtual void Cancel()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }

    public abstract void Execute();
}
