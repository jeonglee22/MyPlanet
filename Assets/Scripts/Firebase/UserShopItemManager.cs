using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Collections.Generic;
using System;
using System.Globalization;

public class UserShopItemManager : MonoBehaviour
{
    private static UserShopItemManager instance;
    public static UserShopItemManager Instance => instance;

    private UserShopItemData buyedShopItemData;
    public UserShopItemData BuyedShopItemData => buyedShopItemData;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private DatabaseReference shopRef;
    private DatabaseReference metaRef;
    [SerializeField] private bool useLocalTimeForTest = false;

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
        metaRef = FirebaseDatabase.DefaultInstance.RootReference.Child(DatabaseRef.UserMetaData);

        isInitialized = true;
    }

    public async UniTask<bool> LoadUserShopItemDataAsync()
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
                var result = await InitUserShopItemDataAsync();
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

    public async UniTask<bool> InitUserShopItemDataAsync()
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

    public async UniTask<bool> SaveUserShopItemDataAsync(List<bool> buyedItem, bool packageShop, List<BuyItemData> buyedItems)
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

    private async UniTask<long> GetServerNowMsAsync(string uid)
    {
        var lastSeenRef = metaRef.Child(uid).Child("lastSeenAt");

        if (useLocalTimeForTest)
        {
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await lastSeenRef.SetValueAsync(time).AsUniTask();
            var snapLocal = await lastSeenRef.GetValueAsync().AsUniTask();
            return ToLong(snapLocal.Value);
        } 

        await lastSeenRef.SetValueAsync(ServerValue.Timestamp).AsUniTask();
        var snap = await lastSeenRef.GetValueAsync().AsUniTask();

        return ToLong(snap.Value);
    }

    private static long ToLong(object v)
    {
        if (v is long l) return l;
        if (v is int i) return i;
        if (v is double d) return (long)d;
        if (long.TryParse(v?.ToString(), out var parsed)) return parsed;
        return 0;
    }

    public async UniTask<bool> EnsureDailyShopFreshAsync()
{
    if (!AuthManager.Instance.IsSignedIn)
        return false;

    var uid = AuthManager.Instance.UserId;

    long serverNowMs = await GetServerNowMsAsync(uid);
    string todayKey = ToDayKeyKst(serverNowMs);

    var userShopRef = shopRef.Child(uid);

    if (buyedShopItemData == null)
        buyedShopItemData = new UserShopItemData();

    var snap = await userShopRef.RunTransaction(mutable =>
    {
        var currentKey = mutable.Child("lastResetDayKey").Value?.ToString();
        if (currentKey == todayKey)
            return TransactionResult.Abort(); 

        mutable.Child("lastResetDayKey").Value = todayKey;
        mutable.Child("todaySeed").Value = serverNowMs;
        mutable.Child("isUsedReroll").Value = false;

        var daily = new List<object>(6);
        for (int i = 0; i < 6; i++) daily.Add(false);
        mutable.Child("dailyShop").Value = daily;

        var items = new List<object>(6);
        for (int i = 0; i < 6; i++)
        {
            items.Add(new Dictionary<string, object>
            {
                { "itemId", 0 },
                { "count", 0 }
            });
        }
        mutable.Child("buyedItems").Value = items;

        return TransactionResult.Success(mutable);
    }).AsUniTask();

    var newSeedObj = snap.Child("todaySeed").Value;
    long newSeed = 0;
    if (newSeedObj is long l) newSeed = l;
    else if (newSeedObj is int i) newSeed = i;
    else if (newSeedObj is double d) newSeed = (long)d;
    else long.TryParse(newSeedObj?.ToString(), out newSeed);

    bool didReset = (newSeed == serverNowMs);
    if (!didReset)
        return false;

    buyedShopItemData.lastResetDayKey = todayKey;
    buyedShopItemData.todaySeed = serverNowMs;
    buyedShopItemData.isUsedReroll = false;

    buyedShopItemData.dailyShop = new List<bool>(6);
    for (int i = 0; i < 6; i++) buyedShopItemData.dailyShop.Add(false);

    buyedShopItemData.buyedItems = new List<BuyItemData>(6);
    for (int i = 0; i < 6; i++) buyedShopItemData.buyedItems.Add(new BuyItemData());

    return true;
}

    private static string ToDayKeyKst(long serverNowMs)
    {
        var tz = GetKst();
        var utc = DateTimeOffset.FromUnixTimeMilliseconds(serverNowMs);
        var local = TimeZoneInfo.ConvertTime(utc, tz);
        return local.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo GetKst()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul"); }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"); }
            catch { return TimeZoneInfo.Local; }
        }
    }
}
