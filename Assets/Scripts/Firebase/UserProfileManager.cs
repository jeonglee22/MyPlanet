using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
public class UserProfileManager : MonoBehaviour
{
    private UserProfileManager instance;
    public UserProfileManager Instance => instance;

    private UserProfile currentProfile;
    public UserProfile CurrentProfile => currentProfile;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private DatabaseReference userRef;

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

        userRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");

        isInitialized = true;
    }

    public async UniTask<bool> LoadUserProfileAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var dataSnapshot = await userRef.Child(uid).GetValueAsync().AsUniTask();

            if(!dataSnapshot.Exists)
            {
                Debug.LogError("User profile does not exist.");
                return false;
            }
            
            var json = dataSnapshot.GetRawJsonValue();
            currentProfile = UserProfile.FromJson(json);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveUserProfileAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        var email = AuthManager.Instance.UserEmail;
        var nickName = AuthManager.Instance.UserNickName;
        
        try
        {

            var newProfile = new UserProfile(nickName, email);
            var json = newProfile.ToJson();

            await userRef.Child(uid).SetValueAsync(json).AsUniTask();

            currentProfile = newProfile;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> ProfileExistAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var dataSnapshot = await userRef.Child(uid).GetValueAsync().AsUniTask();

            return dataSnapshot.Exists;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }
}