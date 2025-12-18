using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RandomAbilityReinforceUpgradeData
{
    public int RandomAbilityReinforceUpgrade_ID { get; set; }
    public int RandomAbilityReinforceUpgradeLevel { get; set; }
    public int SpecialEffect1_ID { get; set; }
    public float SpecialEffect1AddValue { get; set; }
    public int SpecialEffect2_ID { get; set; }
    public float SpecialEffect2AddValue { get; set; }
    public int SpecialEffect3_ID { get; set; }
    public float SpecialEffect3AddValue { get; set; }
    public float SuperSpecialEffectValue { get; set; }
}

public class RandomAbilityReinforceUpgradeTable : DataTable
{
    private readonly Dictionary<int, RandomAbilityReinforceUpgradeData> dictionary
        = new Dictionary<int, RandomAbilityReinforceUpgradeData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<RandomAbilityReinforceUpgradeData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.RandomAbilityReinforceUpgrade_ID, item))
            {
                Debug.LogError($"Å° Áßº¹: {item.RandomAbilityReinforceUpgrade_ID}");
            }
        }
    }

    public RandomAbilityReinforceUpgradeData Get(int upgradeId)
    {
        return dictionary.TryGetValue(upgradeId, out var data) ? data : null;
    }

    public bool TryGet(int upgradeId, out RandomAbilityReinforceUpgradeData data)
    {
        return dictionary.TryGetValue(upgradeId, out data);
    }

    public RandomAbilityReinforceUpgradeData GetFromIdArray(int[] reinforceUpgradeIds, int level)
    {
        if (reinforceUpgradeIds == null || reinforceUpgradeIds.Length == 0)
            return null;

        if (level <= 0) level = 1;
        var index = level - 1;
        if (index >= reinforceUpgradeIds.Length)
            index = reinforceUpgradeIds.Length - 1;

        return Get(reinforceUpgradeIds[index]);
    }

    public List<RandomAbilityReinforceUpgradeData> GetAll()
    {
        return new List<RandomAbilityReinforceUpgradeData>(dictionary.Values);
    }
}