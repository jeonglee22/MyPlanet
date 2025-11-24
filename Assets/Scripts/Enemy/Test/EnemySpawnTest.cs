using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private int enemyId;

    private ScaleData scaleData = new ScaleData()
    {
        HpScale = 1f,
        AttScale = 1f,
        DefScale = 1f,
        MoveSpeedScale = 1f,
        PenetScale = 1f,
        PrefabScale = 1f
    };

    public void SpawnEnemy()
    {
        Vector3 spawnPosition = spawner.transform.position + new Vector3(0f, 2f, 0f);
        
        Enemy enemy = spawner.SpawnEnemyWithScale(enemyId, spawnPosition, scaleData);
    }
}
