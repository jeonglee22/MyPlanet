using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

[Serializable]
public class UserPlanetInfo
{
    public string name;
    public bool owned;
    public int level;
    public int starLevel;
    public int pieceId;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserPlanetInfo FromJson(string json)
    {
        return JsonUtility.FromJson<UserPlanetInfo>(json);
    }
}

[Serializable]
public class UserPlanetsData
{
    public int activePlanetId;
    public Dictionary<string, UserPlanetInfo> planets;
}

public class PlanetManager : MonoBehaviour
{
    private static PlanetManager instance;
    public static PlanetManager Instance => instance;

    private DatabaseReference planetsRef;
    private DatabaseReference activePlanetRef;

    private UserPlanetsData userPlanetsData;
    private bool isDirty = false;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    public int ActivePlanetId => userPlanetsData?.activePlanetId ?? -1;

    public UserPlanetInfo ActivePlanet
    {
        get
        {
            if(userPlanetsData == null || userPlanetsData.activePlanetId < 0)
            {
                return null;
            }

            string planetKey = userPlanetsData.activePlanetId.ToString();
            return userPlanetsData.planets.TryGetValue(planetKey, out var planetInfo) ? planetInfo : null;
        }
    }

    private const int maxLevel = 50;
    private const int maxStarLevel = 5;
    public int MaxLevel => maxLevel;
    public int MaxStarLevel => maxStarLevel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid Start()
    {
        await FireBaseInitializer.Instance.WaitInitialization();
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        if (AuthManager.Instance.IsSignedIn)
        {
            InitializeReference();
            var activePlanetSnapshot = await activePlanetRef.GetValueAsync().AsUniTask();
            var planetsSnapshot = await planetsRef.GetValueAsync().AsUniTask();

            if(activePlanetSnapshot.Exists && planetsSnapshot.Exists)
            {
                await LoadPlanetsAsync();
            }
            else
            {
                InitializePlanetsData();
                isDirty = true;
                await SavePlanetsAsync();
            }
        }

        isInitialized = true;
    }

    private void InitializeReference()
    {
        string userId = AuthManager.Instance.UserId;
        DatabaseReference userDataRef = FirebaseDatabase.DefaultInstance.RootReference.Child("userdata").Child(userId);

        activePlanetRef = userDataRef.Child("activePlanetId");
        planetsRef = userDataRef.Child("planets");
    }

    private void InitializePlanetsData()
    {
        var allPlanetData = DataTableManager.PlanetTable.GetAll();

        if(allPlanetData == null || allPlanetData.Count == 0)
        {
            Debug.LogError("[Planet] PlanetTable에 데이터가 없습니다.");
            return;
        }

        userPlanetsData = new UserPlanetsData
        {
            activePlanetId = allPlanetData[0].Planet_ID,
            planets = new Dictionary<string, UserPlanetInfo>()
        };

        for(int i = 0; i < allPlanetData.Count; i++)
        {
            var planetData = allPlanetData[i];
            string planetKey = planetData.Planet_ID.ToString();

            userPlanetsData.planets[planetKey] = new UserPlanetInfo
            {
                name = planetData.PlanetName,
                owned = i == 0,
                level = 0,
                starLevel = 0,
                pieceId = planetData.PieceId
            };

            if(i == 0)
            {
                userPlanetsData.planets[planetKey].starLevel = 1;
            }
        }
    }

    public void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    public async UniTask<(bool success, string error)> LoadPlanetsAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "사용자가 로그인되어 있지 않습니다.");
        }

        try
        {
            var activePlanetSnapshot = await activePlanetRef.GetValueAsync().AsUniTask();

            var planetsSnapshot = await planetsRef.GetValueAsync().AsUniTask();

            if(activePlanetSnapshot.Exists && planetsSnapshot.Exists)
            {
                userPlanetsData = new UserPlanetsData
                {
                    activePlanetId = int.Parse(activePlanetSnapshot.Value.ToString()),
                    planets = new Dictionary<string, UserPlanetInfo>()
                };

                foreach (var planetChild in planetsSnapshot.Children)
                {
                    string planetKey = planetChild.Key;
                    string planetJson = planetChild.GetRawJsonValue();
                    var planetInfo = UserPlanetInfo.FromJson(planetJson);
                    userPlanetsData.planets[planetKey] = planetInfo;
                }
            }
            else
            {
                InitializePlanetsData();
                isDirty = true;
                await SavePlanetsAsync();
            }

            isDirty = false;
            return (true, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Planet] 행성 데이터 로드 실패: {e.Message}");
            return (false, e.Message);
        }
    }

    public async UniTask<(bool success, string error)> SavePlanetsAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "사용자가 로그인되어 있지 않습니다.");
        }

        if(!isDirty)
        {
            return (true, null);
        }

        try
        {
            await activePlanetRef.SetValueAsync(userPlanetsData.activePlanetId.ToString()).AsUniTask();

            foreach (var kvp in userPlanetsData.planets)
            {
                string planetKey = kvp.Key;
                string json = kvp.Value.ToJson();

                await planetsRef.Child(planetKey).SetRawJsonValueAsync(json).AsUniTask();
            }

            isDirty = false;
            return (true, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Planet] 행성 데이터 저장 실패: {e.Message}");
            return (false, e.Message);
        }
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public void LevelUpPlanet(int planetId)
    {
        string planetKey = planetId.ToString();
        if(userPlanetsData?.planets != null && userPlanetsData.planets.TryGetValue(planetKey, out var planetInfo))
        {
            planetInfo.level++;
            MarkDirty();
        }
    }

    public void StarUpPlanet(int planetId)
    {
        string planetKey = planetId.ToString();
        if(userPlanetsData?.planets != null && userPlanetsData.planets.TryGetValue(planetKey, out var planetInfo))
        {
            planetInfo.starLevel++;
            MarkDirty();
        }
    }

    public void SetActivePlanet(int planetId)
    {
        string planetKey = planetId.ToString();
        if(userPlanetsData?.planets != null && userPlanetsData.planets.ContainsKey(planetKey))
        {
            userPlanetsData.activePlanetId = planetId;
            MarkDirty();
        }
    }

    public UserPlanetInfo GetPlanetInfo(int planetId)
    {
        string planetKey = planetId.ToString();
        if(userPlanetsData?.planets != null && userPlanetsData.planets.TryGetValue(planetKey, out var planetInfo))
        {
            return planetInfo;
        }
        return null;
    }

    public Dictionary<string, UserPlanetInfo> GetAllPlanets()
    {
        return userPlanetsData?.planets;
    }

    public void UnlockPlanet(int planetId)
    {
        string planetKey = planetId.ToString();
        if(userPlanetsData?.planets != null && userPlanetsData.planets.TryGetValue(planetKey, out var planetInfo))
        {
            planetInfo.owned = true;
            planetInfo.starLevel = 1;
            MarkDirty();
        }
    }

    public bool HasPlanet(int planetId)
    {
        string planetKey = planetId.ToString();
        if(userPlanetsData?.planets != null && userPlanetsData.planets.TryGetValue(planetKey, out var planetInfo))
        {
            return planetInfo.owned;
        }
        return false;
    }

    public void ClearPlanetData()
    {
        userPlanetsData = null;
        isDirty = false;
        isInitialized = false;
        planetsRef = null;
        activePlanetRef = null;
    }

    public async UniTask ReloadPlanetsForNewUser()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return;
        }

        InitializeReference();

        var activePlanetSnapshot = await activePlanetRef.GetValueAsync().AsUniTask();
        var planetsSnapshot = await planetsRef.GetValueAsync().AsUniTask();

        if(activePlanetSnapshot.Exists && planetsSnapshot.Exists)
        {
            await LoadPlanetsAsync();
        }
        else
        {
            InitializePlanetsData();
            isDirty = true;
            await SavePlanetsAsync();
        }

        isInitialized = true;
    }
}
