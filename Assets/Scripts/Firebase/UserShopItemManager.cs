using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Collections.Generic;

public class UserShopItemManager : MonoBehaviour
{
    private static UserShopItemManager instance;
    public static UserShopItemManager Instance => instance;

    private UserShopItemData buyedShopItemData;
    public UserShopItemData BuyedShopItemData => buyedShopItemData;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private DatabaseReference shopRef;

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

        shopRef = FirebaseDatabase.DefaultInstance.RootReference.Child(DatabaseRef.UserShopItemData);

        isInitialized = true;
    }

    public async UniTask<bool> LoadUserStageClearAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var dataSnapshot = await shopRef.Child(uid).GetValueAsync().AsUniTask();

            if(!dataSnapshot.Exists)
            {
                // Debug.LogError("User Stage Clear Data does not exist.");
                var result = await InitUserStageClearAsync();
                return result;
            }
            
            var json = dataSnapshot.GetRawJsonValue();
            buyedShopItemData = UserShopItemData.FromJson(json);

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
            var newStageData = new UserShopItemData();
            var json = newStageData.ToJson();

            await shopRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();
            buyedShopItemData = newStageData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveUserStageClearAsync(List<bool> buyedItem, bool packageShop, List<BuyItemData> buyedItems)
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var newStageData = new UserShopItemData(buyedItem, packageShop, buyedItems);
            var json = newStageData.ToJson();

            await shopRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            buyedShopItemData = newStageData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveUserShopItemDataAsync(UserShopItemData shopItemData)
    {
        if(!AuthManager.Instance.IsSignedIn)
            return false;

        var uid = AuthManager.Instance.UserId;
        
        try
        {
            var json = shopItemData.ToJson();

            await shopRef.Child(uid).SetRawJsonValueAsync(json).AsUniTask();

            buyedShopItemData = shopItemData;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load user profile: {ex.Message}");
            return false;
        }
    }
}
