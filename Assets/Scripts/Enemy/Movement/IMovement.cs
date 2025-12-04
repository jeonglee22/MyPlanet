using UnityEngine;

public interface IMovement
{
    public void Initialize(int enemyType);
    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target);
    public void OnPatternLine();
    public bool IsCompleted();
    public bool IsPatternLine { get; }
}
