using UnityEngine;

public interface IPattern
{
    public int PatternId { get; }
    public ExecutionTrigger Trigger { get; }
    public float TriggetValue { get; } //Interval

    public void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data);
    public void Execute();
    public bool CanExecute();
    public void Reset();
    public void PatternUpdate();
}
