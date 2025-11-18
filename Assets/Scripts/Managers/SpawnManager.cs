using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance { get { return instance; } }

    private List<EnemySpawner> spawnPoints = new List<EnemySpawner>();

    [SerializeField] private GameObject spawnPointSample;
    [SerializeField] private int spawnPointCount = 5;

    public int CurrentEnemyCount { get => currentEnemyCount; }
    private int currentEnemyCount = 0;

    public bool IsSpawning { get; set; } = false;

    private Rect screenBounds;
    private Rect offSetBounds;
    public Rect ScreenBounds => screenBounds;
    [SerializeField] private float offSet = 10f;

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
        GenerateSemicircleSpawnPoints();
    }

    private void Start()
    {
        ChangeSpawningState();
    }

    public void ChangeSpawningState() => IsSpawning = !IsSpawning;

    public void OnEnemyDied()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0)
        {
            currentEnemyCount = 0;
        }

        if(currentEnemyCount == 0)
        {
            WaveManager.Instance.OnWaveCleared().Forget();
        }
    }

    //Screen bounds for enemy movement
    private void SetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z; // 카메라에서의 거리
        float minHeight = Screen.height * 0.75f;

        var bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, minHeight, zDistance));
        var screenBottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0f, zDistance));
        var topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, zDistance));

        screenBounds = new Rect(screenBottomLeft.x, screenBottomLeft.y, topRight.x - screenBottomLeft.x, topRight.y - screenBottomLeft.y);
        offSetBounds = new Rect(bottomLeft.x - offSet, bottomLeft.y, (topRight.x - bottomLeft.x) + (offSet * 2), (topRight.y - bottomLeft.y) + offSet);
    }

    public void GenerateSemicircleSpawnPoints()
    {
        Vector3 center = new Vector3(offSetBounds.center.x, offSetBounds.yMin, 0f);

        float radius = Mathf.Abs(center.x - offSetBounds.xMin);

        for (int i = 0; i < spawnPointCount; i++)
        {
            float angle = Mathf.PI * i / (spawnPointCount - 1);

            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);

            var spawner = Instantiate(spawnPointSample, new Vector3(x, y, 0f), Quaternion.identity).GetComponent<EnemySpawner>();
            spawnPoints.Add(spawner);

            spawnPoints[i].gameObject.name = "SpawnPoint_" + (i + 1);
        }
    }

    private EnemySpawner GetSpawner(int spawnerIndex)
    {
        int index = spawnerIndex;
        if (spawnerIndex < 0 || spawnerIndex >= spawnPoints.Count)
        {
            return null;
        }

        return spawnPoints[spawnerIndex];
    }

    public void PrepareEnemyPools(int enemyId)
    {
        foreach (var spawner in spawnPoints)
        {
            spawner.PreparePool(enemyId);
        }
    }

    private void SpawnSlot(int enemyId, int quantity, int spawnPointIndex, ScaleData scaleData)
    {
        if(enemyId == 0 || quantity <= 0 || spawnPointIndex - 1 <= 0)
        {
            return;
        }

        var spawner = GetSpawner(spawnPointIndex - 1);
        if(spawner == null)
        {
            return;
        }

        spawner.SpawnEnemiesWithScale(enemyId, quantity, scaleData);
        currentEnemyCount += quantity;
    }

    public void SpawnCombination(CombineData combineData, ScaleData scaleData)
    {
        if(combineData == null)
        {
            return;
        }

        SpawnSlot(combineData.Enemy_Id_1, combineData.EnemyQuantity_1, combineData.SpawnPoint_1, scaleData);
        SpawnSlot(combineData.Enemy_Id_2, combineData.EnemyQuantity_2, combineData.SpawnPoint_2, scaleData);
        SpawnSlot(combineData.Enemy_Id_3, combineData.EnemyQuantity_3, combineData.SpawnPoint_3, scaleData);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(screenBounds.xMin, screenBounds.yMin, 0f), new Vector3(screenBounds.xMin, screenBounds.yMax, 0f));
        Gizmos.DrawLine(new Vector3(screenBounds.xMin, screenBounds.yMax, 0f), new Vector3(screenBounds.xMax, screenBounds.yMax, 0f));
        Gizmos.DrawLine(new Vector3(screenBounds.xMax, screenBounds.yMax, 0f), new Vector3(screenBounds.xMax, screenBounds.yMin, 0f));
        Gizmos.DrawLine(new Vector3(screenBounds.xMax, screenBounds.yMin, 0f), new Vector3(screenBounds.xMin, screenBounds.yMin, 0f));
    }
}
