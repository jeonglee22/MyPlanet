using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SocialPlatforms.GameCenter;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData[] enemyDatas;

    private Transform player;

    private Dictionary<GameObject, IObjectPool<Enemy>> enemyPools = new Dictionary<GameObject, IObjectPool<Enemy>>();
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;

    private float spawnInterval = 2f;
    private float spawnTimer = 0f;
    private int enemyCount = 5;
    private float spawnRadius = 1f;

    private List<GameObject> spawnEnemy = new List<GameObject>();

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Planet").transform;

        for(int i = 0; i < 100; i++)
        {
            var enemy = SpawnEnemy(enemyDatas[Random.Range(0, enemyDatas.Length)], transform.position);
            enemy.gameObject.SetActive(false);
            spawnEnemy.Add(enemy.gameObject);
        }
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
                SpawnEnemy(enemyDatas[Random.Range(0, enemyDatas.Length)], rnadomPosition);
            }

            spawnTimer = 0f;
            spawnInterval = Random.Range(1f, 3f);
            enemyCount = Random.Range(1, 3);
        }
    }
    
    private Vector3 GetRandomPositionInCircle()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
    }

    public Enemy SpawnEnemy(EnemyData data, Vector3 spawnPosition)
    {
        Vector3 direction = CalculateDirection(data.movementType, spawnPosition);
        return CreateEnemy(data, spawnPosition, direction);
    }

    public Enemy SpawnEnemy(EnemyData data, Vector3 spawnPosition, Vector3 targetDirection)
    {
        return CreateEnemy(data, spawnPosition, targetDirection);
    }

    private Vector3 CalculateDirection(MovementType type, Vector3 spawnPos)
    {
        switch (type)
        {
            case MovementType.StraightDown:
                return Vector3.down;
            case MovementType.TargetDirection:
                if (player != null)
                {
                    return (player.position - spawnPos).normalized;
                }
                else
                {
                    return Vector3.down;
                }
            default:
                return Vector3.down;
        }
    }

    private Enemy CreateEnemy(EnemyData data, Vector3 position, Vector3 direction)
    {
        GameObject prefab = data.prefab;
        if(!enemyPools.ContainsKey(prefab))
        {
            CreatePool(prefab);
        }

        Enemy enemy = enemyPools[prefab].Get();
        enemy.transform.position = position;

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
        enemy.transform.rotation = rotation;

        enemy.Initialize(data, direction);
        return enemy;
    }

    private void CreatePool(GameObject prefab)
    {
        IObjectPool<Enemy> pool = new ObjectPool<Enemy>(
            () => CreateEnemyInstance(prefab),
            (enemy) => 
            {
                enemy.SetPool(enemyPools[prefab]);
                enemy.gameObject.SetActive(true);
            },
            (enemy) => enemy.gameObject.SetActive(false),
            (enemy) => Destroy(enemy.gameObject),
            collectionCheck,
            defaultPoolCapacity,
            maxPoolSize
        );

        enemyPools.Add(prefab, pool);
    }

    private Enemy CreateEnemyInstance(GameObject prefab)
    {
        GameObject enemyObj = Instantiate(prefab);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
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
}
