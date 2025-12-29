using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class itemStringData
{
    public string Key { get; set; }
    public string Text { get; set; }
}

public class ItemStringTable : DataTable
{
    private readonly Dictionary<string, itemStringData> dictionary = new Dictionary<string, itemStringData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<itemStringData>(textAsset.text);

        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Key, item))
            {
                Debug.LogError($"키 중복: {item.Key}");
            }
        }
    }

    public string GetString(string key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key].Text;
    }
}