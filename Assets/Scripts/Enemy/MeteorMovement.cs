using UnityEngine;

public class MeteorMovement : EnemyMovement
{
    protected override void Move()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }
}
