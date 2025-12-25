using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class SpecialEffectTextData
{
    public int SpecialEffectText_ID { get; set; }
    public string Name { get; set; }
}

public class SpecialEffectTextTable : DataTable
{
    private readonly Dictionary<int, SpecialEffectTextData> dictionary =
        new Dictionary<int, SpecialEffectTextData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<SpecialEffectTextData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.SpecialEffectText_ID, item))
            {
                Debug.LogError($"[SpecialEffectTable] �ߺ� Ű: {item.SpecialEffectText_ID}");
            }
        }
    }

    public SpecialEffectTextData Get(int specialEffectTextId)
    {
        if (!dictionary.TryGetValue(specialEffectTextId, out var data))
        {
            return null;
        }
        return data;
    }

    public IReadOnlyDictionary<int, SpecialEffectTextData> GetAll() => dictionary;
}