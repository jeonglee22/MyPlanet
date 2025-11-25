using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    private Transform player;

    private ObjectPoolManager<int, Enemy> objectPoolManager = new ObjectPoolManager<int, Enemy>();
    [SerializeField] private bool collectionCheck = true;
    private int defaultPoolCapacity = 100;
    private int maxPoolSize = 1000;

    private float spawnRadius = 1f;

    //test
    private EnemyTableData currentTableData;
    private int spawnPointIndex = -1;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(TagName.Planet).transform;
    }

    public void SetSpawnPointIndex(int index)
    {
        spawnPointIndex = index;
    }
    
    private Vector3 GetRandomPositionInCircle()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
    }

    private Enemy CreateEnemy(int enemyId, Vector3 position, Vector3 direction, ScaleData scaleData)
    {
        Enemy enemy = objectPoolManager.Get(enemyId);
        if (enemy == null)
        {
            return null;
        }

        enemy.transform.position = position;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
        enemy.transform.rotation = rotation;

        enemy.Spawner = this;
        enemy.Initialize(currentTableData, direction, enemyId, objectPoolManager, scaleData, spawnPointIndex);
        return enemy;
    }

    private Enemy CreateChildEnemy(int enemyId, Vector3 position, Vector3 direction, ScaleData scaleData, int moveType)
    {
        Enemy enemy = objectPoolManager.Get(enemyId);
        if (enemy == null)
        {
            return null;
        }

        enemy.transform.position = position;
        enemy.Spawner = this;

        enemy.InitializeAsChild(currentTableData, direction, enemyId, objectPoolManager, scaleData, moveType);
        return enemy;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawCircle(transform.position, spawnRadius, 50);
    }

    
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);
            Vector3 currentPoint = new Vector3(x, y, center.z);
            
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    //test
    public void PreparePool(int enemyId)
    {
        if(GameManager.LoadManagerInstance == null || objectPoolManager.HasPool(enemyId))
        {
            return;
        }

        GameObject prefab = GameManager.LoadManagerInstance.GetLoadedPrefab(enemyId);
        if(prefab == null)
        {
            GameManager.LoadManagerInstance.LoadEnemyPrefabAsync(enemyId).Forget();
        }

        if(prefab == null)
        {
            return;
        }

        objectPoolManager.CreatePool(
            enemyId,
            prefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck,
            this.transform
        );
    }

    public void SpawnEnemiesWithScale(int enemyId, int quantity, ScaleData scaleData)
    {
        PreparePool(enemyId);

        for (int i = 0; i < quantity; i++)
        {
            Vector3 spawnPos = GetRandomPositionInCircle();
            SpawnEnemyWithScale(enemyId, spawnPos, scaleData);
        }
    }

    public Enemy SpawnEnemyWithScale(int enemyId, Vector3 spawnPosition, ScaleData scaleData)
    {
        PreparePool(enemyId);

        currentTableData = DataTableManager.EnemyTable.Get(enemyId);
        if(currentTableData == null)
        {
            return null;
        }

        return CreateEnemy(enemyId, spawnPosition, Vector3.down, scaleData);
    }

    public Enemy SpawnEnemyAsChild(int enemyId, Vector3 spawnPosition, ScaleData scaleData, int moveType)
    {
        return CreateChildEnemy(enemyId, spawnPosition, Vector3.down, scaleData, moveType);
    }

    public void SpawnEnemiesWithMovement(int enemyId, int quantity, ScaleData scaleData, int moveType)
    {
        PreparePool(enemyId);

        currentTableData = DataTableManager.EnemyTable.Get(enemyId);
        if (currentTableData == null)
        {
            return;
        }

        for (int i = 0; i < quantity; i++)
        {
            Vector3 spawnPos = GetRandomPositionInCircle();
            SpawnEnemyWithScale(enemyId, spawnPos, scaleData);
        }
    }
}
