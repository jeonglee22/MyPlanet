using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using NUnit.Framework;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;
    public static ItemManager Instance => instance;

    private DatabaseReference itemsRef;

    private Dictionary<int, int> userItems = new Dictionary<int, int>();
    private HashSet<int> validItemIds = new HashSet<int>();
    
    private bool isDirty = false;
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
        await FireBaseInitializer.Instance.WaitInitialization();

        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        itemsRef = FirebaseDatabase.DefaultInstance.RootReference.Child("userdata");

        InitializeValidItems();

        Debug.Log("[Items] ItemManager 초기화 완료");

        if(AuthManager.Instance.IsSignedIn)
        {
            await LoadItemsAsync();
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void InitializeValidItems()
    {
        validItemIds.Clear();
        userItems.Clear();

        var allItems = DataTableManager.ItemTable.GetAllItemsExceptCollectionItem();

        foreach(var item in allItems)
        {
            int itemId = item.Item_Id;

            validItemIds.Add(itemId);
            userItems[itemId] = 0;
        }

        Debug.Log("[Items] 유효한 아이템 초기화 완료");
    }

    public async UniTask<(bool success, string error)> LoadItemsAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "[Items] 로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log("[Items] 아이템 로드 시도");

            var dataSnapshot = await itemsRef.Child(userId).Child("items").GetValueAsync().AsUniTask();

            if(dataSnapshot.Exists)
            {
                string json = dataSnapshot.GetRawJsonValue();
                ItemsData itemsData = ItemsData.FromJson(json);

                var loadedItems = itemsData.ToDictionary();

                foreach(var kvp in loadedItems)
                {
                    if(validItemIds.Contains(kvp.Key))
                    {
                        userItems[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        Debug.LogWarning($"[Items] 알 수 없는 아이템 ID 무시: {kvp.Key}");
                    }
                }

                Debug.Log("[Items] 아이템 로드 성공");
            }
            else
            {
                Debug.Log("[Items] 아이템 데이터 없음, 새로 생성");

                await SaveItemsAsync();
            }

            isDirty = false;
            return (true, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Items] 아이템 로드 실패: {e.Message}");

            return (false, e.Message);
        }
    }

    public async UniTask<(bool success, string error)> SaveItemsAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "[Items] 로그인 필요");
        }

        if(!isDirty)
        {
            Debug.Log("[Items] 변경사항 없음, 저장 생략");
            return (true, null);
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log("[Items] 아이템 저장 시도");

            ItemsData userItemsData = ItemsData.FromDictionary(userItems);
            string json = userItemsData.ToJson();

            await itemsRef.Child(userId).Child("items").SetRawJsonValueAsync(json).AsUniTask();

            isDirty = false;
            Debug.Log("[Items] 아이템 저장 성공");

            return (true, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Items] 아이템 저장 실패: {e.Message}");
            return (false, e.Message);
        }
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public int GetItem(int itemId)
    {
        if(!validItemIds.Contains(itemId))
        {
            Debug.LogWarning($"[Items] 알 수 없는 itemId: {itemId}");
            return 0;
        }

        return userItems.TryGetValue(itemId, out int value) ? value : 0;
    }

    public void SetItem(int itemId, int value)
    {
        if(!validItemIds.Contains(itemId))
        {
            Debug.LogWarning($"[Items] 알 수 없는 itemId: {itemId}");
            return;
        }

        userItems[itemId] = value;

        MarkDirty();
    }

    public void AddItem(int itemId, int amount)
    {
        if(!validItemIds.Contains(itemId))
        {
            Debug.LogWarning($"[Items] 알 수 없는 itemId: {itemId}");
            return;
        }

        int currentValue = GetItem(itemId);
        int maxStack = GetMaxStack(itemId);
        int newAmount = Mathf.Min(currentValue + amount, maxStack);

        SetItem(itemId, newAmount);
    }

    private int GetMaxStack(int itemId)
    {
        var itemData = DataTableManager.ItemTable.Get(itemId);
        
        return itemData?.MaxStack ?? 9999;
    }
}
