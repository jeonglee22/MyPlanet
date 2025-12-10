using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class UserTowerManager : MonoBehaviour
{
    private static UserTowerManager instance;
    public static UserTowerManager Instance => instance;

    private DatabaseReference userTowerRef;

    private UserTowerData[] currentTowerDatas;
    public UserTowerData[] CurrentTowerDatas => currentTowerDatas;

    private const int towerTypeCount = 6;

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

        userTowerRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserTowers);

        Debug.Log("UserTowerManager initialized.");
        
        isInitialized = true;
    }

    public async UniTask<bool> LoadUserTowerDataAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var userRef = userTowerRef.Child(uid);
            for (int i = 0; i < towerTypeCount; i++)
            {
                string towerKey = i switch
                {
                    0 => "GunTower",
                    1 => "ShootGunTower",
                    2 => "GatlingGunTower",
                    3 => "LazerTower",
                    4 => "SniperTower",
                    5 => "MissileTower",
                    _ => ""
                };

                var dataSnapshot = await userRef.Child(towerKey).GetValueAsync().AsUniTask();

                if(!dataSnapshot.Exists)
                {
                    Debug.LogError($"User tower data for {towerKey} does not exist.");
                    return false;
                }

                var json = dataSnapshot.GetRawJsonValue();
                currentTowerDatas[i] = UserTowerData.FromJson(json);
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user tower data: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> LoadUserTowerDataAsync(string asyncUserId)
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;
        
        try
        {
            var userRef = userTowerRef.Child(asyncUserId);
            for (int i = 0; i < towerTypeCount; i++)
            {
                string towerKey = i switch
                {
                    0 => "GunTower",
                    1 => "ShootGunTower",
                    2 => "GatlingGunTower",
                    3 => "LazerTower",
                    4 => "SniperTower",
                    5 => "MissileTower",
                    _ => ""
                };

                var dataSnapshot = await userRef.Child(towerKey).GetValueAsync().AsUniTask();

                if(!dataSnapshot.Exists)
                {
                    Debug.LogError($"User tower data for {towerKey} does not exist.");
                    return false;
                }

                var json = dataSnapshot.GetRawJsonValue();
                currentTowerDatas[i] = UserTowerData.FromJson(json);
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user tower data: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> InitUserTowerDataAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var towerDatas = new UserTowerData[towerTypeCount];
            // var userRef = userTowerRef.Child(uid);
            for (int i = 0; i < towerTypeCount; i++)
            {
                Debug.Log("Initializing tower data for tower index: " + i);
                string towerKey = i switch
                {
                    0 => "GunTower",
                    1 => "ShootGunTower",
                    2 => "GatlingGunTower",
                    3 => "LazerTower",
                    4 => "SniperTower",
                    5 => "MissileTower",
                    _ => ""
                };

                var defaultTowerData = i switch
                {
                    0 => new UserTowerData(1000001),
                    1 => new UserTowerData(1000002),
                    2 => new UserTowerData(1001001),
                    3 => new UserTowerData(1001002),
                    4 => new UserTowerData(1002001),
                    5 => new UserTowerData(1002002),
                    _ => null
                };

                var json = defaultTowerData.ToJson();

                Debug.Log("Default tower data JSON for " + towerKey + ": " + json);
                await userTowerRef.Child(uid).Child(towerKey).SetRawJsonValueAsync(json).AsUniTask();

                Debug.Log(towerKey + " initialized.");
                towerDatas[i] = defaultTowerData;
            }
            currentTowerDatas = towerDatas;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize user tower data: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> UpdateUserTowerDataAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var towerDatas = new Dictionary<string, object>();
            
            // change to usertowerdata
            //

            towerDatas.Add("GunTower", new UserTowerData(1000001));
            towerDatas.Add("ShootGunTower", new UserTowerData(1000002));
            towerDatas.Add("GatlingGunTower", new UserTowerData(1001001));
            towerDatas.Add("LazerTower", new UserTowerData(1001002));
            towerDatas.Add("SniperTower", new UserTowerData(1002001));
            towerDatas.Add("MissileTower", new UserTowerData(1002002));

            //
            //           
            await userTowerRef.Child(uid).UpdateChildrenAsync(towerDatas).AsUniTask();

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save user tower data: {ex.Message}");
            return false;
        }
    }
}