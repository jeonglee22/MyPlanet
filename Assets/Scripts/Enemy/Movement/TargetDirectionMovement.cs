using UnityEngine;

public class TargetDirectionMovement : EnemyMovement
{
    protected override void Move()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
