using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class UserTowerUpgradeManager : MonoBehaviour
{
    private static UserTowerUpgradeManager instance;
    public static UserTowerUpgradeManager Instance => instance;

    private DatabaseReference userTowerUpgradeRef;

    private UserTowerUpgradeData userTowerUpgradeData;
    public UserTowerUpgradeData CurrentTowerUpgradeData => userTowerUpgradeData;

    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

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
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        userTowerUpgradeRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserTowerUpgrades);

        // SetDummyUserPlanetData().Forget();
        Debug.Log("UserTowerUpgradeManager initialized.");
        
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public async UniTask<bool> InitUserTowerUpgradeAsync()
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var newTowerUpgradeData = new UserTowerUpgradeData();
            var json = newTowerUpgradeData.ToJson();
            await userTowerUpgradeRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            userTowerUpgradeData = newTowerUpgradeData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> LoadUserTowerUpgradeAsync()
    {
        if (!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;

        try
        {
            var dataSnapshot = await userTowerUpgradeRef.Child(uid).GetValueAsync().AsUniTask();

            if (!dataSnapshot.Exists)
            {
                await InitUserTowerUpgradeAsync();
                return true;
            }

            var json = dataSnapshot.GetRawJsonValue();
            var newTowerUpgradeData = UserTowerUpgradeData.FromJson(json);

            userTowerUpgradeData = newTowerUpgradeData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error : {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveUserTowerUpgradeAsync(UserTowerUpgradeData userTowerUpgradeData)
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return false;
        }

        var uid = AuthManager.Instance.UserId;

        try
        {
            var json = userTowerUpgradeData.ToJson();
            await userTowerUpgradeRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            this.userTowerUpgradeData = userTowerUpgradeData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error : {ex.Message}");
            return false;
        }
    }
}
