using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class UserPlanetManager : MonoBehaviour
{
    private static UserPlanetManager instance;
    public static UserPlanetManager Instance => instance;

    private DatabaseReference userPlanetRef;

    private List<UserPlanetData> userPlanets = new List<UserPlanetData>();
    public List<UserPlanetData> UserPlanets => userPlanets;

    private UserPlanetData currentPlanet;
    public UserPlanetData CurrentPlanet => currentPlanet;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        userPlanetRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserPlanets);
        Debug.Log("UserPlanetManager initialized.");
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

    public async UniTask<bool> SaveUserPlanetAsync(string nickName, int attackPower)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var planetData = new UserPlanetData(nickName, attackPower);
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

    public async UniTask<bool> GetRandomPlanetsAsync(int count)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        try
        {
            var dataSnapshot = await userPlanetRef.GetValueAsync().AsUniTask();

            var userCount = dataSnapshot.ChildrenCount;

            count = Mathf.Min(count, (int)userCount);

            userPlanets.Clear();
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
                        userPlanets.Add(profile);
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
}
