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
    private List<Enemy> spawnedEnemies = new List<Enemy>();

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

    private Vector3 GetRandomPositionInCircleWithPos(Vector3 pos)
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return pos + new Vector3(randomCircle.x, randomCircle.y, 0f);
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

        spawnedEnemies.Add(enemy);

        enemy.Spawner = this;
        enemy.Initialize(currentTableData, enemyId, objectPoolManager, scaleData, spawnPointIndex);
        return enemy;
    }

    private Enemy CreateChildEnemy(int enemyId, Vector3 position, ScaleData scaleData, int moveType)
    {
        Enemy enemy = objectPoolManager.Get(enemyId);
        if (enemy == null)
        {
            return null;
        }

        spawnedEnemies.Add(enemy);

        enemy.transform.position = position;
        enemy.Spawner = this;

        enemy.InitializeAsChild(currentTableData, enemyId, objectPoolManager, scaleData, moveType);
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
            prefab = GameManager.LoadManagerInstance.LoadEnemyPrefabAsync(enemyId).AsTask().Result;
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

    public void SpawnEnemiesWithScale(int enemyId, int quantity, ScaleData scaleData, Vector3 spawnPos)
    {
        PreparePool(enemyId);

        for (int i = 0; i < quantity; i++)
        {
            SpawnEnemyWithScale(enemyId, spawnPos, scaleData);
        }
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

    public void SpawnEnemiesWithScale(int enemyId, int quantity, ScaleData scaleData, EnemyTableData enemyTableData)
    {
        PreparePool(enemyId);

        for (int i = 0; i < quantity; i++)
        {
            var newSpawnPos = GetRandomPositionInCircle();
            SpawnEnemyWithScale(enemyId, newSpawnPos, scaleData, enemyTableData);
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

    public Enemy SpawnEnemyWithScale(int enemyId, Vector3 spawnPosition, ScaleData scaleData, EnemyTableData enemyTableData)
    {
        PreparePool(enemyId);

        currentTableData = DataTableManager.EnemyTable.Get(enemyId);
        if(currentTableData == null)
        {
            return null;
        }

        if (Variables.IsTestMode)
        {
            currentTableData = enemyTableData;
        }

        return CreateEnemy(enemyId, spawnPosition, Vector3.down, scaleData);
    }

    public Enemy SpawnEnemyAsChild(int enemyId, Vector3 spawnPosition, ScaleData scaleData, int moveType, bool ShouldDropItems = true)
    {
        var enemy = CreateChildEnemy(enemyId, spawnPosition, scaleData, moveType);
        if(enemy != null)
        {
            enemy.ShouldDropItems = ShouldDropItems;
        }
        return enemy;
    }

    public void SpawnEnemiesWithSummon(int enemyId, int quantity, ScaleData scaleData, bool ShouldDropItems = true, Vector3 spawnPos = default)
    {
        PreparePool(enemyId);

        currentTableData = DataTableManager.EnemyTable.Get(enemyId);
        if (currentTableData == null)
        {
            return;
        }

        for (int i = 0; i < quantity; i++)
        {
            var pos = spawnPos == default ? GetRandomPositionInCircle() : GetRandomPositionInCircleWithPos(spawnPos);
            var enemy = SpawnEnemyWithScale(enemyId, pos, scaleData);
            if(enemy != null)
            {
                enemy.ShouldDropItems = ShouldDropItems;
            }
        }
    }

    public void SpawnEnemiesInCircle(int enemyId, int quantity, float radius, ScaleData scaleData, bool ShouldDropItems = true, Vector3 centerPos = default)
    {
        PreparePool(enemyId);

        currentTableData = DataTableManager.EnemyTable.Get(enemyId);
        if (currentTableData == null)
        {
            return;
        }

        for (int i = 0; i < quantity; i++)
        {
            float angle = i * Mathf.PI * 2f / quantity;
            Vector3 spawnPos = centerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            var enemy = SpawnEnemyWithScale(enemyId, spawnPos, scaleData);
            if(enemy != null)
            {
                enemy.ShouldDropItems = ShouldDropItems;
            }
        }
    }

    public void DespawnAllEnemies()
    {
        foreach(var enemy in spawnedEnemies)
        {
            if(enemy != null && (!enemy.IsDead || enemy.gameObject.activeSelf))
            {
                enemy.OnLifeTimeOver();
            }
        }
    }

    public void DespawnAllEnemiesExceptBoss()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && (!enemy.IsDead || enemy.gameObject.activeSelf) && enemy != Variables.LastBossEnemy && enemy != Variables.MiddleBossEnemy)
            {
                enemy.OnLifeTimeOver();
            }
        }
    }
}
