using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class UserAttackPowerManager : MonoBehaviour
{
    private static UserAttackPowerManager instance;
    public static UserAttackPowerManager Instance => instance;

    private DatabaseReference userAttackRef;

    private UserAttackPowerData currentAttackPower;
    public UserAttackPowerData CurrentPlanetPower => currentAttackPower;

    private string similarAttackPowerUserId;
    public string SimilarAttackPowerUserId => similarAttackPowerUserId;

    private bool isNotSimilarUserFound = false;
    public bool IsNotSimilarUserFound => isNotSimilarUserFound;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

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

        userAttackRef = FirebaseDatabase.DefaultInstance.RootReference.Child(DatabaseRef.UserAttackPowers);

        Debug.Log("[Attack Power] End Init");

        isInitialized = true;
    }

    public async UniTask<bool> LoadUserAttackPowerAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var dataSnapshot = await userAttackRef.Child(uid).GetValueAsync().AsUniTask();

            if(!dataSnapshot.Exists)
            {
                await SaveUserAttackPowerAsync(0);
                return true;
            }
            
            var json = dataSnapshot.GetRawJsonValue();
            currentAttackPower = UserAttackPowerData.FromJson(json);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LoadUserAttackPowerAsync failed: {e.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveUserAttackPowerAsync(int attackPower)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var attackPowerData = new UserAttackPowerData(attackPower, 0);
            var json = attackPowerData.ToJson();

            await userAttackRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveUserAttackPowerAsync failed: {e.Message}");
            return false;
        }
    }

    public async UniTask<bool> FindSimilarAttackPowerUserAsync()
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        try
        {
            var userTowerRef = FirebaseDatabase.DefaultInstance.RootReference.Child(DatabaseRef.UserTowers);
            var dataSnapshot = await userTowerRef.GetValueAsync().AsUniTask();

            var userCount = dataSnapshot.ChildrenCount;
            int index = Random.Range(0, (int)userCount);

            int start = 0;
            foreach (var child in dataSnapshot.Children)
            {
                if (index == start)
                {
                    var json = child.GetRawJsonValue();
                    var profile = UserPlanetData.FromJson(json);
                    similarAttackPowerUserId = child.Key;
                    break;
                }
                start++;
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FindSimilarAttackPowerUserAsync failed: {e.Message}");
            return false;
        }


        // ------------------------------------------------------------------------------ //
        try
        {
            var dataSnapshot = await userAttackRef.GetValueAsync().AsUniTask();

            if (!dataSnapshot.Exists)
            {
                Debug.LogError("No attack power data found.");
                return false;
            }

            similarAttackPowerUserId = string.Empty;
            int aboveDifference = 300;
            int belowDifference = -300;

            var currentPower = currentAttackPower.attackPower;
            var similarList = new List<string>();

            while (true)
            {
                similarList.Clear();

                foreach (var childSnapshot in dataSnapshot.Children)
                {
                    var json = childSnapshot.GetRawJsonValue();
                    var userAttackPowerData = UserAttackPowerData.FromJson(json);

                    var userPower = userAttackPowerData.attackPower;

                    if ((userPower > currentPower + aboveDifference) || (userPower < currentPower + belowDifference))
                        continue;

                    similarList.Add(childSnapshot.Key);
                }

                if (similarList.Count > 0)
                {
                    int randomIndex = Random.Range(0, similarList.Count);
                    similarAttackPowerUserId = similarList[randomIndex];
                    
                    return true;
                }

                belowDifference -= 200;

                if (belowDifference < 0)
                {
                    isNotSimilarUserFound = true;
                    return false;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FindSimilarAttackPowerUserAsync failed: {e.Message}");
            return false;
        }
    }
}
