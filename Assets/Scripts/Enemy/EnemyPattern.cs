using System.Threading;
using UnityEngine;

public enum ExecutionTrigger
{
    None,
    OnPatternLine,
    OnInterval,
}

public abstract class EnemyPattern : MonoBehaviour
{
    protected Enemy owner;
    protected EnemyMovement movement;
    protected EnemyTableData data;
    protected CancellationTokenSource cts;

    public virtual void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        owner = enemy;
        this.movement = movement;
        data = enemyData;
    }

    public virtual float CalculateDamage(float damage)
    {
        return damage;
    }

    protected virtual void Update()
    {
        
    }

    public virtual void OnTrigger(Collider other)
    {
        
    }

    protected virtual void Cancel()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }
}
