using UnityEngine;

public class TargetDirectionMovement : EnemyMovement
{
    protected override void Move()
    {
        if(!isDirectionSet && player != null)
        {
            SetTargetDirection();
            isDirectionSet = true;
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    protected override void SetTargetDirection()
    {
        Vector3 direction = player.position - transform.position;
        moveDirection = direction.normalized;
    }

    private void OnEnable()
    {
        isDirectionSet = false;
    }
}
