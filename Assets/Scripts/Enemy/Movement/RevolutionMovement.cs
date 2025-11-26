using UnityEngine;

public class RevolutionMovement : IMovement
{
    private float offSet = 2f;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private Transform lastBoss;
    private float revolutionRadius;
    private float currentAngle;

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if(lastBoss == null)
        {
            var boss = Variables.LastBossEnemy;
            if(boss == null)
            {
                return Vector3.zero;
            }

            lastBoss = boss.transform;

            var bossCollider = lastBoss.GetComponent<CircleCollider2D>();
            if(bossCollider != null)
            {
                var extents = bossCollider.bounds.extents;
                float colliderRadius = Mathf.Max(extents.x, extents.y);
                revolutionRadius = colliderRadius + offSet;
            }
            else
            {
                revolutionRadius = 3f;
            }
        }

        //currentAngle += 

        return Vector3.zero;
    }

    public void Initialize()
    {
        isPatternLine = false;
    }

    public bool IsCompleted() => false;

    public void OnPatternLine()
    {
        
    }
}
