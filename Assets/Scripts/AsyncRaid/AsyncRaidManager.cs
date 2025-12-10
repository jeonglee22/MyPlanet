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

    void Update()
    {
        if(WaveManager.Instance.IsLastBoss && !isSettingAsyncUserPlanet && canStartSpawn
            && !CameraManager.Instance.IsZoomedOut)
        // if(WaveManager.Instance.IsLastBoss && !isSettingAsyncUserPlanet && canStartSpawn && IsStartRaid)
        {
            var bossHp = Variables.LastBossEnemy != null ? Variables.LastBossEnemy.maxHp : 1;
            SpawnAsyncUserPlanet(bossHp).Forget();
            canStartSpawn = false;
        }
        else if (WaveManager.Instance.IsLastBoss && isSettingAsyncUserPlanet && !isActiveUI)
        {
            asyncRaidUI.gameObject.SetActive(true);
            isActiveUI = true;
        }
    }

    private async UniTask SpawnAsyncUserPlanet(float bossHp)
    {
        userPlanetCount = Random.Range(1, 4);
        // userPlanetCount = 3;
        await UserPlanetManager.Instance.GetRandomPlanetsAsync(userPlanetCount);

        userPlanetData = UserPlanetManager.Instance.CurrentPlanet;

        var asyncUserPlanetObj = Instantiate(asyncUserPlanetPrefab, GetSpawnPoint(), Quaternion.identity);
        var userPlanet = asyncUserPlanetObj.GetComponent<AsyncUserPlanet>();
        var asyncPlanetData = DataTableManager.AsyncPlanetTable.GetRandomData();
        var towerData = towerDataSOs[asyncPlanetData.TowerType - 1];
        userPlanet.InitializePlanet(userPlanetData ?? null, bossHp * totalBossDamagePercent / userPlanetCount, asyncPlanetData, towerData);

        isSettingAsyncUserPlanet = true;
    }

    private Vector3 GetSpawnPoint()
    {
        var screenBounds = SpawnManager.Instance.ScreenBounds;
        var position = Vector3.zero;

        position.y = Random.Range(Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.4f) , Mathf.Lerp(screenBounds.yMin, screenBounds.yMax, 0.7f));
        if (Random.value < 0.5f)
        {
            position.x = screenBounds.xMin - xOffset;
        }
        else
        {
            position.x = screenBounds.xMax + xOffset;
        }

        return position;
    }
}
