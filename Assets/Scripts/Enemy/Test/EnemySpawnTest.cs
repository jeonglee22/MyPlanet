using TMPro;
using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private int enemyId;
    [SerializeField] TMP_Dropdown enemyDropdown;

    private ScaleData scaleData = new ScaleData()
    {
        HpScale = 1f,
        AttScale = 1f,
        DefScale = 1f,
        MoveSpeedScale = 1f,
        PenetScale = 1f,
        PrefabScale = 1f,
        ExpScale = 1f
    };

    private void Start()
    {
        spawner.transform.position = SpawnManager.Instance.Spawners[2].transform.position;
    }

    void OnEnable()
    {
        enemyDropdown.onValueChanged.AddListener((i) => SetEnemyId(i));
        enemyId = 400102;

        var bossObject = GameObject.FindGameObjectWithTag("Boss");
        Variables.TestBossEnemyObject = bossObject;
        // Debug.Log($"{bossObject.transform.position}");
    }

    public void SpawnEnemy()
    {
        Vector3 spawnPosition = spawner.transform.position + new Vector3(0f, 2f, 0f);
        if (Variables.IsTestMode)
        {
            spawnPosition = spawner.transform.position;
            spawner.SpawnEnemiesWithScale(enemyId, 1, scaleData, spawnPosition);
            return;
        }
        
        spawner.SpawnEnemiesWithScale(enemyId, 1, scaleData);
    }

    public void SetEnemyId(int id)
    {
        switch (id)
        {
            case 0:
                enemyId = 400102;
                break;
            case 1:
                enemyId = 400106;
                break;
            case 2:
                enemyId = 400201;
                break;
            case 3:
                enemyId = 400203;
                break;
            case 4:
                enemyId = 400301;
                break;
            case 5:
                enemyId = 400302;
                break;
            case 6:
                enemyId = 400401;
                break;
            case 7:
                enemyId = 400206;
                break;
            case 8:
                enemyId = 400402;
                break;
            case 9:
                enemyId = 400403;
                break;
            case 10:
                enemyId = 409999;
                break;
            case 11:
                enemyId = 408888;
                break;
        }
    }

    public void ClearAllEnemies()
    {
        SpawnManager.Instance.DespawnAllEnemies();
        spawner.DespawnAllEnemies();
    }
}
