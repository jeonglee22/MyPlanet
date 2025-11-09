using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    private static EnemySpawner instance;
    public static EnemySpawner Instance { get { return instance; } }

    [SerializeField] private Transform player;

    private Dictionary<GameObject, IObjectPool<Enemy>> enemyPools = new Dictionary<GameObject, IObjectPool<Enemy>>();
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
            createFunc: () => CreateEnemyInstance(prefab),
            actionOnGet: (enemy) => 
            {
                enemy.SetPool(enemyPools[prefab]);
                enemy.gameObject.SetActive(true);
            },
            actionOnRelease: (enemy) => enemy.gameObject.SetActive(false),
            actionOnDestroy: (enemy) => Destroy(enemy.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultPoolCapacity,
            maxSize: maxPoolSize
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
