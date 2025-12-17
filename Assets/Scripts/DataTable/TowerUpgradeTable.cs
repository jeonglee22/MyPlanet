using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerUpgradeData
{
    public int TowerExternalStatUpgrade_ID { get; set; }
    public int AttackTower_ID { get; set; }
    public int SpecialEffect_ID { get; set; }
    public float SpecialEffectValue { get; set; }
    public int UpgradeCount { get; set; }
    public int GoldCost { get; set; }
    public int MaterialCost { get; set; }
}

public class TowerUpgradeTable : DataTable
{
    private readonly Dictionary<int, TowerUpgradeData> dictionary = new Dictionary<int, TowerUpgradeData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<TowerUpgradeData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.TowerExternalStatUpgrade_ID, item))
            {
                Debug.LogError($"키 중복: {item.TowerExternalStatUpgrade_ID}");
            }
        }
    }

    public TowerUpgradeData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public int GetIdByTowerIdAndUpgradeCount(int towerId, int upgradeCount)
    {
        foreach (var item in dictionary.Values)
        {
            if (item.AttackTower_ID == towerId && item.UpgradeCount == upgradeCount)
            {
                return item.TowerExternalStatUpgrade_ID;
            }
        }

        return -1;
    }
}