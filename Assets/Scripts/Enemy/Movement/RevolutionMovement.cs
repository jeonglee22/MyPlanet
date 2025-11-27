using System;
using UnityEngine;

public class RevolutionMovement : IMovement
{
    private float offSet = 10f;
    private float angleSpeed = 2f;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private Transform lastBoss;
    private float revolutionRadius;
    private float currentAngle;

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if(lastBoss == null && lastBoss == null)
        {
            var boss = Variables.LastBossEnemy;
            if(boss == null)
            {
                return Vector3.zero;
            }

            lastBoss = boss.transform;

            var bossCollider = lastBoss.GetComponent<Collider>();
            if(bossCollider != null)
            {
                var extents = bossCollider.bounds.extents;
                float colliderRadius = extents.magnitude;
                revolutionRadius = colliderRadius + offSet;
            }
            else
            {
                revolutionRadius = 3f;
            }
        }

        currentAngle += angleSpeed * Time.deltaTime;

        Vector3 centerPosition = lastBoss.position;
        float x = Mathf.Cos(currentAngle) * revolutionRadius;
        float y = Mathf.Sin(currentAngle) * revolutionRadius;

        Vector3 targetPosition = new Vector3(centerPosition.x + x, centerPosition.y + y, ownerTransform.position.z);

        Vector3 direction = targetPosition - ownerTransform.position;

        return direction.normalized;
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
