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

    private UserTowerData[] asyncUserTowerDatas;
    public UserTowerData[] AsyncUserTowerDatas => asyncUserTowerDatas;

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

        await LoadUserTowerDataAsync();

        Debug.Log(currentTowerDatas);
    }

    public async UniTask<bool> LoadUserTowerDataAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var userRef = userTowerRef.Child(uid);
            if (currentTowerDatas == null)
                currentTowerDatas = new UserTowerData[towerTypeCount];

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

    public async UniTask<bool> LoadAsyncUserTowerDataAsync(string asyncUserId)
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

                    asyncUserTowerDatas[i] = defaultTowerData;
                    continue;
                }

                var json = dataSnapshot.GetRawJsonValue();
                asyncUserTowerDatas[i] = UserTowerData.FromJson(json);
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

    public async UniTask<bool> UpdateUserTowerDataAsync(UserTowerData[] updatedTowerDatas)
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var towerDatas = new Dictionary<string, object>();
            
            // change to usertowerdata
            //

            foreach (var towerData in updatedTowerDatas)
            {
                string towerKey = towerData.towerId switch
                {
                    1000001 => "GunTower",
                    1000002 => "ShootGunTower",
                    1001001 => "GatlingGunTower",
                    1001002 => "LazerTower",
                    1002001 => "SniperTower",
                    1002002 => "MissileTower",
                    _ => ""
                };

                towerDatas.Add(towerKey, towerData);
                await userTowerRef.Child(uid).Child(towerKey).SetRawJsonValueAsync(towerData.ToJson()).AsUniTask();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save user tower data: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> UpdateUserTowerDataAsync(TowerInstallControl installControl)
    {
        ResetCurrentUserTowerData();

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            var tower = installControl.GetAttackTower(i);
            if (tower == null) 
                continue;

            var projectileData = tower.CurrentProjectileData;
            var towerId = tower.AttackTowerData.towerIdInt;
            var towerLevel = tower.ReinforceLevel;
            var abilities = tower.Abilities;
            var userTowerData = new UserTowerData(towerId, towerLevel, 0, projectileData, abilities);

            var index = towerId switch
            {
                1000001 => 0,
                1000002 => 1,
                1001001 => 2,
                1001002 => 3,
                1002001 => 4,
                1002002 => 5,
                _ => -1
            };

            currentTowerDatas[index] = userTowerData;
        }

        try
        {
            var result = await UpdateUserTowerDataAsync(currentTowerDatas);

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating user tower data: {e.Message}");
            return false;
        }

    }

    private void ResetCurrentUserTowerData()
    {
        currentTowerDatas = new UserTowerData[towerTypeCount];

        for (int i = 0; i < towerTypeCount; i++)
        {
            var towerData = i switch
            {
                0 => new UserTowerData(1000001),
                1 => new UserTowerData(1000002),
                2 => new UserTowerData(1001001),
                3 => new UserTowerData(1001002),
                4 => new UserTowerData(1002001),
                5 => new UserTowerData(1002002),
                _ => null
            };

            currentTowerDatas[i] = towerData;
        }
    }
}