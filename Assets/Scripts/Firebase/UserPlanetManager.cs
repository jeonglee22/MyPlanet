using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class UserPlanetManager : MonoBehaviour
{
    private static UserPlanetManager instance;
    public static UserPlanetManager Instance => instance;

    private DatabaseReference userPlanetRef;

    private UserPlanetData currentPlanet;
    public UserPlanetData CurrentPlanet => currentPlanet;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private int dummyDataCount = 10;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
    }

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        userPlanetRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserPlanets);

        SetDummyUserPlanetData().Forget();
        Debug.Log("UserPlanetManager initialized.");
        
        isInitialized = true;
    }

    public async UniTask<bool> LoadUserPlanetAsync()
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var dataSnapshot = await userPlanetRef.Child(uid).GetValueAsync().AsUniTask();

            if (!dataSnapshot.Exists)
            {
                Debug.LogError("User planet not exist.");
                return false;
            }

            var json = dataSnapshot.GetRawJsonValue();
            var planetData = UserPlanetData.FromJson(json);
            currentPlanet = planetData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> InitUserPlanetAsync(string nickName)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var planetData = new UserPlanetData(nickName);
            var json = planetData.ToJson();

            await userPlanetRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> UpdateUserPlanetAsync(UserPlanetData planetData)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            if (planetData == null)
            {
                Debug.LogError("Planet data is null.");
                return false;
            }

            var updateData = new Dictionary<string, object>
            {
                { "nickName", planetData.nickName },
                { "planetId", planetData.planetId },
                { "planetUpgrade", planetData.planetUpgrade },
                { "planetLevel", planetData.planetLevel },
                { "planetCollectionStat", planetData.planetCollectionStat },
                { "towerId", planetData.towerId }
            };

            await userPlanetRef.Child(uid).UpdateChildrenAsync(updateData).AsUniTask();

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> GetRandomPlanetsAsync(int count)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        try
        {
            var dataSnapshot = await userPlanetRef.GetValueAsync().AsUniTask();

            var userCount = dataSnapshot.ChildrenCount;

            count = Mathf.Min(count, (int)userCount);

            var randIndices = new List<int>();
            int elements = 0;
            while (elements < count)
            {
                var randIndex = Random.Range(0, (int)userCount);
                if (randIndices.Contains(randIndex))
                    continue;
                
                int start = 0;
                foreach (var child in dataSnapshot.Children)
                {
                    if (randIndex == start)
                    {
                        var json = child.GetRawJsonValue();
                        var profile = UserPlanetData.FromJson(json);
                        currentPlanet = profile;
                        randIndices.Add(randIndex);
                        elements++;
                        break;
                    }
                    start++;
                }
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> GetRandomPlanetAsync()
    {
        try
        {
            var result = await GetRandomPlanetsAsync(1);
            return result;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SetDummyUserPlanetData()
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        try
        {
            var dataSnapshot = await userPlanetRef.GetValueAsync().AsUniTask();

            var userCount = dataSnapshot.ChildrenCount;
            Debug.Log(userCount);

            if (userCount >= dummyDataCount)
            {
                Debug.Log("Already have enough dummy data.");
                return true;
            }

            var remainCount = dummyDataCount - (int)userCount;
            for (int i = 0; i < remainCount; i++)
            {
                var nickName = $"DummyUser 100{i}";
                var attackPower = Random.Range(100, 1000);
                var data = new UserPlanetData(nickName, attackPower);
                var json = data.ToJson();

                await userPlanetRef.Push().SetRawJsonValueAsync(json).AsUniTask();
            }
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error : {ex.Message}");
            return false;
        }
    }
}
