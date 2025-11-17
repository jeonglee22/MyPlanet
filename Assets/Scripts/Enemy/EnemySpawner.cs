using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    private Transform player;

    private ObjectPoolManager<int, Enemy> objectPoolManager = new ObjectPoolManager<int, Enemy>();
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;

    private float spawnInterval = 2f;
    private float spawnTimer = 0f;
    private int enemyCount = 1;
    private float spawnRadius = 1f;

    //test
    private EnemyTableData currentTableData;

    private async UniTaskVoid Start()
    {
        player = GameObject.FindGameObjectWithTag("Planet").transform;

        /*
        int enemyDataCount = 0; //test
        foreach (var enemyData in enemyDatas)
        {
            objectPoolManager.CreatePool(
                enemyDataCount++,
                enemyData.prefab,
                initialSize: defaultPoolCapacity,
                maxSize: maxPoolSize,
                collectionCheck: collectionCheck,
                parent: this.transform
            );
        }
        */

        await UniTask.WaitUntil(() => GameManager.LoadManagerInstance != null);
        await UniTask.WaitUntil(() => GameManager.LoadManagerInstance.GetLoadedPrefab(400201) != null);
        CreatePoolFromLoadedPrefab(400201);
    }

    private void Update()
    {
        TrySpawnEnemies();
    }

    private void TrySpawnEnemies()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 rnadomPosition = GetRandomPositionInCircle();
                SpawnEnemy(400201, rnadomPosition);
            }

            spawnTimer = 0f;
            spawnInterval = Random.Range(1f, 3f);
            //enemyCount = Random.Range(1, 3);
        }
    }
    
    private Vector3 GetRandomPositionInCircle()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
    }

    public Enemy SpawnEnemy(int enemyId, Vector3 spawnPosition, bool excutePattern = true)
    {
        currentTableData = DataTableManager.EnemyTable.Get(enemyId);

        if(currentTableData == null)
        {
            return null;
        }

        return CreateEnemy(enemyId, spawnPosition, Vector3.down, excutePattern);
    }

    private Enemy CreateEnemy(int enemyId, Vector3 position, Vector3 direction, bool excutePattern)
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
        enemy.Initialize(currentTableData, direction, enemyId, objectPoolManager, excutePattern);
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
    private void CreatePoolFromLoadedPrefab(int enemyId)
    {
        if(GameManager.LoadManagerInstance == null)
        {
            return;
        }

        GameObject prefab = GameManager.LoadManagerInstance.GetLoadedPrefab(enemyId);

        if(prefab == null || objectPoolManager.HasPool(enemyId))
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
}
