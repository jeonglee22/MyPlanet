using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class UserAttackPowerManager : MonoBehaviour
{
    private static UserPlanetManager instance;
    public static UserPlanetManager Instance => instance;

    private DatabaseReference userAttackRef;

    private UserAttackPowerData currentAttackPower;
    public UserAttackPowerData CurrentPlanetPower => currentAttackPower;

    private int similarAttackPowerUserId;
    public int SimilarAttackPowerUserId => similarAttackPowerUserId;

    private bool isNotSimilarUserFound = false;
    public bool IsNotSimilarUserFound => isNotSimilarUserFound;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private async UniTaskVoid Start()
    {
        await FireBaseInitializer.Instance.WaitInitialization();

        userAttackRef = FirebaseDatabase.DefaultInstance.RootReference.Child("attackPower");

        Debug.Log("[Attack Power] End Init");
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
                Debug.LogError("User attack power does not exist.");
                return false;
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
            var attackPowerData = new UserAttackPowerData(attackPower, (long)ServerValue.Timestamp);
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

    public async UniTask<bool> FindSimilarAttackPowerUserAsync(int attackPower)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        try
        {
            var dataSnapshot = await userAttackRef.GetValueAsync().AsUniTask();

            if (!dataSnapshot.Exists)
            {
                Debug.LogError("No attack power data found.");
                return false;
            }

            similarAttackPowerUserId = -1;
            int aboveDifference = 300;
            int belowDifference = -300;

            var currentPower = currentAttackPower.attackPower;
            var similarList = new List<int>();

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

                    similarList.Add(int.Parse(childSnapshot.Key));
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
