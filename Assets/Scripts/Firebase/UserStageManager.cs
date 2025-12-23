using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase.Database;

public class UserStageManager : MonoBehaviour
{
    private static UserStageManager instance;
    public static UserStageManager Instance => instance;

    private UserStageData clearedStageData;
    public UserStageData ClearedStageData => clearedStageData;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private DatabaseReference stageRef;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private async UniTaskVoid Start()
    {
        await FireBaseInitializer.Instance.WaitInitialization();

        stageRef = FirebaseDatabase.DefaultInstance.RootReference.Child(DatabaseRef.UserStageData);

        isInitialized = true;
    }

    public async UniTask<bool> LoadUserStageClearAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var dataSnapshot = await stageRef.Child(uid).GetValueAsync().AsUniTask();

            if(!dataSnapshot.Exists)
            {
                // Debug.LogError("User Stage Clear Data does not exist.");
                var result = await InitUserStageClearAsync();
                return result;
            }
            
            var json = dataSnapshot.GetRawJsonValue();
            clearedStageData = UserStageData.FromJson(json);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> InitUserStageClearAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var newStageData = new UserStageData();
            var json = newStageData.ToJson();

            await stageRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();
            clearedStageData = newStageData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveUserStageClearAsync(int clearedStage)
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var newStageData = new UserStageData(clearedStage);
            var json = newStageData.ToJson();

            await stageRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            clearedStageData = newStageData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }
}
