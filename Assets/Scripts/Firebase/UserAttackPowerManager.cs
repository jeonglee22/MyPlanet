using System.Collections.Generic;
using System.Threading;
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

    private string similarAttackPowerUserId = string.Empty;
    public string SimilarAttackPowerUserId
    { get { return similarAttackPowerUserId;} set { similarAttackPowerUserId = value; } }

    private bool isNotSimilarUserFound = false;
    public bool IsNotSimilarUserFound => isNotSimilarUserFound;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    public float PlanetPower { get; set; } = 0f;
    public float TowerPower { get; set; } = 0f;

    private const int TowerCount = 6;

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
            await UpdatePlanetPower(UserPlanetManager.Instance.CurrentPlanet);
            await UpdateTowerPower();

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

    public async UniTask<bool> FindSimilarAttackPowerUserAsync(CancellationToken token)
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        // try
        // {
        //     var userTowerRef = FirebaseDatabase.DefaultInstance.RootReference.Child(DatabaseRef.UserTowers);
        //     var dataSnapshot = await userTowerRef.GetValueAsync().AsUniTask();

        //     var userCount = dataSnapshot.ChildrenCount;

        //     var existDataList = new List<string>();
        //     foreach (var child in dataSnapshot.Children)
        //     {
        //         var key = child.Key;

        //         if (token.IsCancellationRequested)
        //         {
        //             Debug.Log("FindSimilarAttackPowerUserAsync cancelled.");
        //             return false;
        //         }

        //         var result = await UserTowerManager.Instance.ExistTowerDataAsync(key);
        //         if (result)
        //         {
        //             existDataList.Add(key);
        //         }
        //     }
        //     // Debug.Log(existDataList.Count + " / " + userCount);

        //     int index = Random.Range(0, existDataList.Count);
        //     similarAttackPowerUserId = existDataList[index];

        //     return true;
        // }
        // catch (System.Exception e)
        // {
        //     Debug.LogError($"FindSimilarAttackPowerUserAsync failed: {e.Message}");
        //     return false;
        // }

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

            bool isLast = false;

            while (true)
            {
                similarList.Clear();

                foreach (var childSnapshot in dataSnapshot.Children)
                {
                    if (childSnapshot.Key == AuthManager.Instance.UserId)
                        continue;

                    var json = childSnapshot.GetRawJsonValue();
                    var userAttackPowerData = UserAttackPowerData.FromJson(json);

                    var userPower = userAttackPowerData.attackPower;
                    if ((userPower > currentPower + aboveDifference) || (userPower < currentPower + belowDifference))
                        continue;

                    var checkTowerExist = await UserTowerManager.Instance.ExistTowerDataAsync(childSnapshot.Key);
                    if (!checkTowerExist)
                        continue;
                    
                    similarList.Add(childSnapshot.Key);
                }

                Debug.Log("Similar Users Found: " + similarList.Count + " (Range: " + belowDifference + " to " + aboveDifference + ")");
                if (similarList.Count > 0)
                {
                    int randomIndex = Random.Range(0, similarList.Count);
                    similarAttackPowerUserId = similarList[randomIndex];
                    
                    return true;
                }

                belowDifference -= 100;
                if (currentPower + belowDifference <= 0 && !isLast)
                {
                    belowDifference = -(currentPower)-1;
                    isLast = true;
                    continue;
                }
                else if (currentPower + belowDifference <= 0 && isLast)
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

    public async UniTask UpdatePlanetPower(UserPlanetData planetData)
    {
        await CalculatePlanetPower(planetData.planetId, planetData.planetLevel, planetData.planetUpgrade);

        SaveUserAttackPowerAsync((int)(PlanetPower + TowerPower)).Forget();
    }

    public async UniTask UpdateTowerPower()
    {
        await CalculateTowerPower();

        SaveUserAttackPowerAsync((int)(PlanetPower + TowerPower)).Forget();
    }

    private async UniTask CalculatePlanetPower(int planetId, int planetLevel, int planetUpgrade)
    {
        await UniTask.WaitUntil(() => DataTableManager.IsInitialized);
        await UniTask.WaitUntil(() => UserPlanetManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => PlanetStatManager.Instance.IsInitialized);

        var planetData = PlanetStatManager.Instance.CalculatePlanetStats(planetId, planetLevel, planetUpgrade);

        var baseAttack = planetData.hp * (100 + planetData.defense) * 0.01f;
        PlanetPower = baseAttack + planetData.shield + planetData.hpRegeneration * 420f + planetData.drain * 550f;
    }

    private async UniTask CalculateTowerPower()
    {
        await UniTask.WaitUntil(() => DataTableManager.IsInitialized);
        await UniTask.WaitUntil(() => UserTowerUpgradeManager.Instance.IsInitialized);

        TowerPower = 0f;
        for (int i = 0; i < TowerCount; i++)
        {
            var towerId = i switch
            {
                0 => AttackTowerId.basicGun,
                1 => AttackTowerId.Gattling,
                2 => AttackTowerId.Missile,
                3 => AttackTowerId.ShootGun,
                4 => AttackTowerId.Sniper,
                5 => AttackTowerId.Lazer,
                _ => throw new System.ArgumentOutOfRangeException(nameof(i), "Invalid tower index")
            };

            var towerUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
            Debug.Log(towerUpgradeData);
            if (towerUpgradeData == null)
                continue;

            var towerIndex = towerUpgradeData.towerIds.IndexOf((int)towerId);
            if (towerIndex == -1)
                continue;
            Debug.Log(towerIndex);
            var attackTowerLevel = towerUpgradeData.upgradeLevels[towerIndex];

            TowerPower += attackTowerLevel switch
            {
                0 => 0f,
                1 => 5f,
                2 => 5f + 7.5f,
                3 => 5f + 7.5f + 10f,
                4 => 5f + 7.5f + 10f + 80f,
                _ => -1f,
            };
        }
    }
}
