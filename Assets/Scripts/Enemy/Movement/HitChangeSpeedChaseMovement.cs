using UnityEngine;

public class HitChangeSpeedChaseMovement : IMovement
{
    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private bool isDirectionSet = false;
    private bool hasHit = false;

    public void Initialize(Enemy owner)
    {
        isDirectionSet = false;
        hasHit = false;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        Vector3 chaseDirection = (target.position - ownerTransform.position).normalized;
        ownerTransform.LookAt(ownerTransform.position + chaseDirection);

        return chaseDirection;
    }

    public void OnHitedInMovement() => hasHit = true;

    public float GetSpeedMultiplier() => hasHit ? 3f : 1f;

    public bool IsCompleted() => isDirectionSet;

    public void OnPatternLine()
    {
        
    }
}
