using UnityEngine;

public class StraightDownMovement : EnemyMovement
{
    protected override void Move()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }
}
