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

    Rect screenBounds;
    [SerializeField] private float offSet = 1f;

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

        SetScreenBounds();
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
        //Vector3 position = GetRandomSpawnPoint();
        Vector3 position = new Vector3(
            Random.Range(screenBounds.xMin, screenBounds.xMax),
            Random.Range(screenBounds.yMin, screenBounds.yMax),
            0f);
        
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
        //Vector3 position = GetRandomSpawnPoint();
        Vector3 position = new Vector3(
            Random.Range(screenBounds.xMin, screenBounds.xMax),
            Random.Range(screenBounds.yMin, screenBounds.yMax),
            0f);

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

    /// <summary>
    /// Screen bounds for enemy movement
    /// </summary>
    private void SetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z; // 카메라에서의 거리
        float minHeight = Screen.height * 0.75f;

        var bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0 + offSet, minHeight, zDistance));
        var topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width + offSet, Screen.height + offSet, zDistance));

        screenBounds = new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
    }
}
