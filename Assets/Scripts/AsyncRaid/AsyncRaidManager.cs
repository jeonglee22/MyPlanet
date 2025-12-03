using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AsyncRaidManager : MonoBehaviour
{
    [SerializeField] private GameObject asyncUserPlanetPrefab;
    [SerializeField] private List<TowerDataSO> towerDataSOs;

    [SerializeField] private int userCount;
    [SerializeField] private AsyncRaidUI asyncRaidUI;

    private List<AsyncUserPlanet> asyncUserPlanets;
    public List<AsyncUserPlanet> AsyncUserPlanets => asyncUserPlanets;
    private List<UserPlanetData> userPlanetDatas;

    private int userPlanetCount;

    private float totalBossDamagePercent = 0.05f;

    private bool isSettingAsyncUserPlanet = false;
    public bool IsSettingAsyncUserPlanet { get { return isSettingAsyncUserPlanet;} set {isSettingAsyncUserPlanet = value;} }

    private bool canStartSpawn = true;
    public bool CanStartSpawn { get { return canStartSpawn;} set {canStartSpawn = value;} }

    public bool IsStartRaid { get; set; } = false;

    private float xOffset = 1.5f;

    void Update()
    {
        if(WaveManager.Instance.IsBossBattle && !isSettingAsyncUserPlanet && canStartSpawn && IsStartRaid)
        {
            var bossHp = Variables.LastBossEnemy != null ? Variables.LastBossEnemy.maxHp : 1;
            SpawnAsyncUserPlanet(bossHp).Forget();
            canStartSpawn = false;
        }
        else if (WaveManager.Instance.IsBossBattle && isSettingAsyncUserPlanet)
        {
            asyncRaidUI.gameObject.SetActive(true);
        }
    }

    private async UniTask SpawnAsyncUserPlanet(float bossHp)
    {
        userPlanetCount = Random.Range(1, 4);
        // userPlanetCount = 3;
        if (userPlanetDatas != null)
            userPlanetDatas.Clear();
        if (asyncUserPlanets != null)
            asyncUserPlanets.Clear();

        await UserPlanetManager.Instance.GetRandomPlanetsAsync(userPlanetCount);

        userPlanetDatas = new List<UserPlanetData>(userPlanetCount);
        userPlanetDatas = UserPlanetManager.Instance.UserPlanets;
        asyncUserPlanets = new List<AsyncUserPlanet>();

        for (int i = 0; i < userPlanetCount; i++)
        {
            var asyncUserPlanetObj = Instantiate(asyncUserPlanetPrefab, GetSpawnPoint(), Quaternion.identity);
            var asyncUserPlanet = asyncUserPlanetObj.GetComponent<AsyncUserPlanet>();
            var asyncPlanetData = DataTableManager.AsyncPlanetTable.GetRandomData();
            var towerData = towerDataSOs[asyncPlanetData.TowerType - 1];
            asyncUserPlanet.InitializePlanet(userPlanetDatas[i] ?? null, bossHp * totalBossDamagePercent / userPlanetCount, asyncPlanetData, towerData);
            asyncUserPlanets.Add(asyncUserPlanet);
        }

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
