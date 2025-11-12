using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    private Transform player;

    private Dictionary<GameObject, IObjectPool<Enemy>> enemyPools = new Dictionary<GameObject, IObjectPool<Enemy>>();
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;
    
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Planet").transform;
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

}
