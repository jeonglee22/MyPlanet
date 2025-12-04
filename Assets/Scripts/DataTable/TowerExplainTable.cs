using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerExplainData
{
    public int TowerText_ID { get; set; }
    public string TowerName { get; set; }
    public string TowerDescribe { get; set; }

    public override string ToString()
    {
        return $"TowerText_ID: {TowerText_ID}, TowerName: {TowerName}, TowerDescribe: {TowerDescribe}";
    }
}

public class TowerExplainTable : DataTable
{
    private readonly Dictionary<int, TowerExplainData> dictionary = new Dictionary<int, TowerExplainData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<TowerExplainData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.TowerText_ID, item))
            {
                Debug.LogError($"키 중복: {item.TowerText_ID}");
            }
        }
    }

    public TowerExplainData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
