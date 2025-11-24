using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class SpecialEffectCombinationData
{
    public int SpecialEffectCombination_ID { get; set; }
    public string Description { get; set; } //Explain Tower
    public int SpecialEffect1_ID { get; set; }
    public float SpecialEffect1Value { get; set; }

    public int SpecialEffect2_ID { get; set; }
    public float SpecialEffect2Value { get; set; }

    public int SpecialEffect3_ID { get; set; }
    public float SpecialEffect3Value { get; set; }

    public override string ToString()
    {
        return $"{SpecialEffectCombination_ID}, \"{Description}\", " +
               $"({SpecialEffect1_ID}:{SpecialEffect1Value}), " +
               $"({SpecialEffect2_ID}:{SpecialEffect2Value}), " +
               $"({SpecialEffect3_ID}:{SpecialEffect3Value})";
    }
}

public class SpecialEffectCombinationTable : DataTable
{
    private readonly Dictionary<int, SpecialEffectCombinationData> dictionary =
        new Dictionary<int, SpecialEffectCombinationData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<SpecialEffectCombinationData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.SpecialEffectCombination_ID, item))
            {
                Debug.LogError($"[SpecialEffectCombinationTable] Áßº¹ Å°: {item.SpecialEffectCombination_ID}");
            }
        }
    }

    public SpecialEffectCombinationData Get(int combinationId)
    {
        if (!dictionary.TryGetValue(combinationId, out var data))
        {
            return null;
        }
        return data;
    }

    public IReadOnlyDictionary<int, SpecialEffectCombinationData> GetAll() => dictionary;
}
