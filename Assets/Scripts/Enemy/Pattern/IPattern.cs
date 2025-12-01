using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IPattern
{
    public int PatternId { get; }
    public ExecutionTrigger Trigger { get; }
    public float TriggerValue { get; } //Interval
    public bool RequireAsync { get; }

    public void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data);
    public void Execute();
    public UniTask ExecuteAsync(CancellationToken token);
    public bool CanExecute();
    public void Reset();
    public void PatternUpdate();
    public PatternData GetPatternData();
}
