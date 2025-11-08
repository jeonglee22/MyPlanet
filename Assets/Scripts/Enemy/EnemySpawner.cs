using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private static EnemySpawner instance;
    public static EnemySpawner Instance { get { return instance; } }

    [SerializeField] private EnemyData[] enemyDatas;

    [SerializeField] private Transform player;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && enemyDatas.Length > 0)
        {
            SpawnEnemy(enemyDatas[0], transform.position);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2) && enemyDatas.Length > 1)
        {
            SpawnEnemy(enemyDatas[1], transform.position);
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
        GameObject enemyObj = Instantiate(data.prefab, position, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy == null)
        {
            Destroy(enemyObj);
            return null;
        }

        enemy.Initialize(data, direction);
        return enemy;
    }
    
}
