using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AsyncRaidManager : MonoBehaviour
{
    [SerializeField] private GameObject asyncUserPlanetPrefab;

    private List<AsyncUserPlanet> asyncUserPlanets;
    private List<UserPlanetData> userPlanetDatas;

    private int userPlanetCount;

    private float totalBossDamagePercent = 5f;
    private Rect screenBounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => UserPlanetManager.Instance.IsInitialized);

        userPlanetCount = Random.Range(1, 4);
        // userPlanetCount = 3;

        await UserPlanetManager.Instance.GetRandomPlanetsAsync(userPlanetCount);

        userPlanetDatas = new List<UserPlanetData>(userPlanetCount);
        userPlanetDatas = UserPlanetManager.Instance.UserPlanets;
        asyncUserPlanets = new List<AsyncUserPlanet>();

        for (int i = 0; i < userPlanetCount; i++)
        {
            var asyncUserPlanetObj = Instantiate(asyncUserPlanetPrefab, transform);
            var asyncUserPlanet = asyncUserPlanetObj.GetComponent<AsyncUserPlanet>();
            asyncUserPlanet.InitializePlanet(userPlanetDatas[i] ?? null, totalBossDamagePercent / userPlanetCount);
            asyncUserPlanets.Add(asyncUserPlanet);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GetScreenBounds()
    {
        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z; // 카메라에서의 거리
        float minHeight = Screen.height * 0.75f;

        var bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, minHeight, zDistance));
        var screenBottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0f, zDistance));
        var topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, zDistance));

        screenBounds = new Rect(screenBottomLeft.x, screenBottomLeft.y, topRight.x - screenBottomLeft.x, topRight.y - screenBottomLeft.y);
    }
}
