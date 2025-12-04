using System;
using UnityEngine;

public class RevolutionMovement : IMovement
{
    private Transform lastBossTransform;
    private float revolutionRadius;
    private float offSet = 4f;

    private bool isRightTransform = false;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private int enemyType;

    public void Initialize(int enemyType)
    {
        isPatternLine = false;

        Enemy lastBoss = Variables.LastBossEnemy;
        if(lastBoss != null)
        {
            lastBossTransform = lastBoss.transform;

            var bossCollider = lastBossTransform.GetComponent<Collider>();
            if(bossCollider != null)
            {
                float maxSize = Mathf.Max(bossCollider.bounds.extents.x, bossCollider.bounds.extents.y, bossCollider.bounds.extents.z);
                revolutionRadius = (maxSize / 2f) + offSet;
            }
            else
            {
                revolutionRadius = 3f;
            }

            isRightTransform = false;
        }

        this.enemyType = enemyType;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if(lastBossTransform == null)
        {
            return baseDirection;
        }

        Vector3 direction = ownerTransform.position - lastBossTransform.position;
        float currentRadius = direction.magnitude;

        if(!isRightTransform || Mathf.Abs(currentRadius - revolutionRadius) > 0.1f)
        {
            Vector3 targetPosition = lastBossTransform.position + direction.normalized * revolutionRadius;
            Vector3 toTargetDirection = (targetPosition - ownerTransform.position).normalized;

            if(Mathf.Abs(currentRadius - revolutionRadius) < 0.5f)
            {
                isRightTransform = true;
            }

            return toTargetDirection;
        }

        Vector3 toCircleDirection = Vector3.Cross(direction, Vector3.forward).normalized;
        return toCircleDirection;
    }

    public bool IsCompleted() => false;

    public void OnPatternLine()
    {
        
    }
}
