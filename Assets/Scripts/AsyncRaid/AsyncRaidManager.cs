using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AsyncRaidManager : MonoBehaviour
{
    [SerializeField] private GameObject asyncUserPlanetPrefab;
    [SerializeField] private List<TowerDataSO> towerDataSOs;

    [SerializeField] private int userCount;
    [SerializeField] private AsyncRaidUI asyncRaidUI;

    private AsyncUserPlanet userPlanet;
    public AsyncUserPlanet UserPlanet => userPlanet;
    private UserPlanetData userPlanetData;

    private int userPlanetCount;

    private float totalBossDamagePercent = 0.05f;

    private bool isSettingAsyncUserPlanet = false;
    public bool IsSettingAsyncUserPlanet { get { return isSettingAsyncUserPlanet;} set {isSettingAsyncUserPlanet = value;} }

    private bool canStartSpawn = true;
    public bool CanStartSpawn { get { return canStartSpawn;} set {canStartSpawn = value;} }

    public bool IsStartRaid { get; set; } = false;

    private float xOffset = 1.5f;
    private bool isActiveUI = false;
    private float asyncUserSpawnTimer;

    void Update()
    {
        if(WaveManager.Instance.IsLastBoss && !isSettingAsyncUserPlanet && canStartSpawn
            && !CameraManager.Instance.IsZoomedOut)
        // if(WaveManager.Instance.IsLastBoss && !isSettingAsyncUserPlanet && canStartSpawn && IsStartRaid)
        {
            asyncUserSpawnTimer += Time.deltaTime;
            if(asyncUserSpawnTimer < 3f)
                return;

            var bossHp = Variables.LastBossEnemy != null ? Variables.LastBossEnemy.maxHp : 1;
            SpawnAsyncUserPlanetAsync(bossHp).Forget();
            canStartSpawn = false;
        }
        else if (WaveManager.Instance.IsLastBoss && isSettingAsyncUserPlanet && !isActiveUI)
        {
            asyncRaidUI.gameObject.SetActive(true);
            isActiveUI = true;
        }
    }

    // private async UniTask SpawnAsyncUserPlanet(float bossHp)
    // {
    //     await UserPlanetManager.Instance.GetRandomPlanetAsync();

    //     userPlanetData = UserPlanetManager.Instance.CurrentPlanet;

    //     var spawnPos = GetSpawnPoint();
    //     var asyncUserPlanetObj = Instantiate(asyncUserPlanetPrefab, spawnPos, Quaternion.identity);
    //     userPlanet = asyncUserPlanetObj.GetComponent<AsyncUserPlanet>();
    //     var asyncPlanetData = DataTableManager.AsyncPlanetTable.GetRandomData();
    //     var towerData = towerDataSOs[asyncPlanetData.TowerType - 1];
    //     userPlanet.InitializePlanet(userPlanetData ?? null, bossHp * totalBossDamagePercent, asyncPlanetData, towerData, GetReflectPoint(spawnPos));

    //     isSettingAsyncUserPlanet = true;
    // }

    private async UniTask SpawnAsyncUserPlanetAsync(float bossHp)
    {
        await UserAttackPowerManager.Instance.FindSimilarAttackPowerUserAsync();

        var similarUserId = UserAttackPowerManager.Instance.SimilarAttackPowerUserId;

        await UserTowerManager.Instance.LoadAsyncUserTowerDataAsync(similarUserId);
        await UserPlanetManager.Instance.LoadAsyncUserPlanetAsync(similarUserId);

        Debug.Log(similarUserId + "Loadaed Async User Data");
        userPlanetData = UserPlanetManager.Instance.AsyncUserPlanet;
        var spawnPos = GetSpawnPoint();
        var asyncUserPlanetObj = Instantiate(asyncUserPlanetPrefab, spawnPos, Quaternion.identity);
        userPlanet = asyncUserPlanetObj.GetComponent<AsyncUserPlanet>();
        var asyncUserTowerDatas = UserTowerManager.Instance.AsyncUserTowerDatas;

        userPlanet.InitializePlanet(userPlanetData ?? null, bossHp, asyncUserTowerDatas, GetReflectPoint(spawnPos));
    
        isSettingAsyncUserPlanet = true;
    }

    private Vector3 GetSpawnPoint()
    {
        var screenBounds = SpawnManager.Instance.ScreenBounds;
        var position = Vector3.zero;

        var yCenter = Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.4f);

        var leftAboveArea = new Rect(screenBounds.xMin - xOffset* 1.5f, yCenter, xOffset, yCenter - Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.2f));
        var rightAboveArea = new Rect(screenBounds.xMax + xOffset, yCenter, xOffset, yCenter - Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.2f));
        var leftBelowArea = new Rect(screenBounds.xMin - xOffset * 1.5f, Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.2f), xOffset, yCenter - Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.2f));
        var rightBelowArea = new Rect(screenBounds.xMax + xOffset, Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.2f), xOffset, yCenter - Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.2f));

        var randomArea = Random.value switch
        {
            < 0.25f => leftAboveArea,
            < 0.5f => rightAboveArea,
            < 0.75f => leftBelowArea,
            _ => rightBelowArea
        };
        Debug.Log("Spawn Area : " + randomArea);

        position.x = Random.Range(randomArea.xMin, randomArea.xMax);
        position.y = Random.Range(randomArea.yMin, randomArea.yMax);
        position.z = 0f;

        return position;
    }

    private Vector3 GetReflectPoint(Vector3 spawnPoint)
    {
        var xCenter = (SpawnManager.Instance.ScreenBounds.xMin + SpawnManager.Instance.ScreenBounds.xMax) / 2f;
        var yCenter = (SpawnManager.Instance.ScreenBounds.yMin + SpawnManager.Instance.ScreenBounds.yMax) / 2f;

        var reflectPoint = spawnPoint;
        if(spawnPoint.x < xCenter)
        {
            reflectPoint.x += 2 * (xCenter - spawnPoint.x);
        }
        else
        {
            reflectPoint.x -= 2 * (spawnPoint.x - xCenter);
        }

        if(spawnPoint.y < yCenter)
        {
            reflectPoint.y += 2 * (yCenter - spawnPoint.y);
        }
        else
        {
            reflectPoint.y -= 2 * (spawnPoint.y - yCenter);
        }

        return reflectPoint;
    }
}
