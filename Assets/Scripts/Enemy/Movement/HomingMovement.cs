using UnityEngine;

public class HomingMovement : EnemyMovement
{
    protected override void Move()
    {
        base.Move();
        if(!isDirectionSet)
        {
            SetTargetDirection();
            isDirectionSet = true;
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        transform.LookAt(transform.position + moveDirection);
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
