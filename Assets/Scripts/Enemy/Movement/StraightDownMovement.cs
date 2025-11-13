using UnityEngine;

public class StraightDownMovement : EnemyMovement
{
    private Vector3 targetDirection;

    protected override void Move()
    {
        if (!isDirectionSet)
        {
            SetTargetDirection();
            isDirectionSet = true;
        }
        transform.position += targetDirection * moveSpeed * Time.deltaTime;
    }

    protected override void SetTargetDirection()
    {
        var screenRect = SpawnManager.Instance.ScreenBounds;
        var randomPosition = new Vector3(Random.Range(screenRect.xMin, screenRect.xMax), screenRect.yMin, 0f);

        targetDirection = (randomPosition - transform.position).normalized;
    }

    private void OnEnable()
    {
        isDirectionSet = false;
    }
}
