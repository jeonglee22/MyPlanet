using UnityEngine;

public class StraightDownMovement : EnemyMovement
{
    private Vector3 targetDirection;
    private int spawnPointIndex = -1;

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
        Vector3 randomPosition;

        if (spawnPointIndex == 1)
        {
            float quarterWidth = screenRect.width * 0.25f;
            randomPosition = new Vector3(Random.Range(screenRect.xMin, screenRect.xMin + quarterWidth), screenRect.yMin, 0f);
        }
        else if (spawnPointIndex == 3)
        {
            float quarterWidth = screenRect.width * 0.25f;
            randomPosition = new Vector3(Random.Range(screenRect.xMax - quarterWidth, screenRect.xMax), screenRect.yMin, 0f);
        }
        else
        {
            randomPosition = new Vector3(Random.Range(screenRect.xMin, screenRect.xMax), screenRect.yMin, 0f);
        }

        targetDirection = (randomPosition - transform.position).normalized;
    }

    public override void SetDirection(Vector3 direction)
    {
        targetDirection = direction.normalized;
    }

    private void OnEnable()
    {
        isDirectionSet = false;
    }

    public void SetSpawnPointIndex(int index)
    {
        spawnPointIndex = index;
    }
}
