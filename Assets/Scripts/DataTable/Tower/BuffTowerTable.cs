using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class BuffTowerData
{
    public int BuffTower_ID { get; set; }
    public string BuffTowerName { get; set; }
    public int SlotNum { get; set; }
    public int SpecialEffectCombination_ID { get; set; }
    public int RandomAbilityGroup_ID { get; set; }

    public override string ToString()
    {
        return $"{BuffTower_ID}, {BuffTowerName}, SlotNum={SlotNum}, " +
               $"Comb={SpecialEffectCombination_ID}, RandomGroup={RandomAbilityGroup_ID}";
    }
}

public class BuffTowerTable : DataTable
{
    private readonly Dictionary<int, BuffTowerData> dictionary = new Dictionary<int, BuffTowerData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<BuffTowerData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.BuffTower_ID, item))
            {
                Debug.LogError($"[BuffTowerTable] Áßº¹ Å°: {item.BuffTower_ID}");
            }
        }
    }

    public BuffTowerData Get(int buffTowerId)
    {
        if (!dictionary.TryGetValue(buffTowerId, out var data))
        {
            return null;
        }
        return data;
    }

    public IReadOnlyDictionary<int, BuffTowerData> GetAll() => dictionary;
}
