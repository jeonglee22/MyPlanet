using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance { get { return instance; } }

    [SerializeField] private EnemyData[] enemyDatas;
    private List<GameObject> spawnPoints = new List<GameObject>();

    [SerializeField] private GameObject spawnPointSample;
    [SerializeField] private int spawnPointCount = 5;

    public int CurrentEnemyCount { get => currentEnemyCount; }
    private int currentEnemyCount = 0;

    public bool IsSpawning { get; set; } = false;

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

    public void GenerateSemicircleSpawnPoints()
    {
        Vector3 center = new Vector3(screenBounds.center.x, screenBounds.yMin, 0f);

        float radius = Mathf.Abs(center.x - screenBounds.xMin);

        for (int i = 0; i < spawnPointCount; i++)
        {
            float angle = Mathf.PI * i / (spawnPointCount - 1);

            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);

            spawnPoints.Add(Instantiate(spawnPointSample, new Vector3(x, y, 0f), Quaternion.identity));
        }
    }
}
