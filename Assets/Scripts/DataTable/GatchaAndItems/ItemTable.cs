using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemData
{
    public int Item_Id { get; set; }
    public int ItemType { get; set; }
    public int UseCondition { get; set; }
    public int MaxStack { get; set; }
    public string ItemName { get; set; }
    public string ItemNameText { get; set; }
    public string ItemDescription { get; set; }
    public string ItemDescriptionText { get; set; }

    public override string ToString()
    {
        return $"Item_Id: {Item_Id}, Name: {ItemName}, ItemType: {ItemType}, UseCondition: {UseCondition}, MaxStack: {MaxStack}, Description: {ItemDescription}";
    }
}

public class ItemTable : DataTable
{
    private readonly Dictionary<int, ItemData> dictionary = new Dictionary<int, ItemData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<ItemData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Item_Id, item))
            {
                Debug.LogError($"키 중복: {item.Item_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public ItemData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }
}
