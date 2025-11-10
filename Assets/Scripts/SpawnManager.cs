using NUnit.Framework;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance { get { return instance; } }

    [SerializeField] private EnemyData[] enemyDatas;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private float spawnInterval = 1f;
    private float spawnTimer = 0f;

    private int currentEnemyCount = 0;
    public int CurrentEnemyCount => currentEnemyCount;

    public bool IsSpawning { get; set; } = true;

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

    private void Start()
    {
        StartSpawning();
    }

    private void Update()
    {
        if (!IsSpawning)
        {
            return;
        }

        if (spawnTimer < spawnInterval)
        {
            spawnTimer += Time.deltaTime;
        }
        else
        {
            SpawnRandomEnemy();
            spawnTimer = 0f;
        }
    }

    public void StartSpawning()
    {
        IsSpawning = true;
        spawnTimer = 0f;
    }
    
    public void StopSpawning()
    {
        IsSpawning = false;
    }

    public Enemy SpawnRandomEnemy()
    {
        EnemyData data = GetRandomEnemyData();
        Vector3 position = GetRandomSpawnPoint();
        
        Enemy enemy = EnemySpawner.Instance.SpawnEnemy(data, position);
        if (enemy != null)
        {
            currentEnemyCount++;
        }
        
        return enemy;
    }

    //Use split ability
    public Enemy SpawnEnemy(EnemyData data)
    {
        Vector3 position = GetRandomSpawnPoint();

        Enemy enemy = EnemySpawner.Instance.SpawnEnemy(data, position);
        if (enemy != null)
        {
            currentEnemyCount++;
        }

        return enemy;
    }

    public void OnEnemyDied()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0)
        {
            currentEnemyCount = 0;
        }
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return new Vector3(0, 10, 0);
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex].position;
    }
    
    private EnemyData GetRandomEnemyData()
    {
        if (enemyDatas == null || enemyDatas.Length == 0)
        {
            return null;
        }
        
        int randomIndex = Random.Range(0, enemyDatas.Length);
        return enemyDatas[randomIndex];
    }
}
