using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerUpgradeAbilityUnlockData
{
    public int TowerUpgradeAbilityUnlock_ID { get; set; }
    public int AttackTower_ID { get; set; }
    public int RandomAbility_ID { get; set; }
    public int RandomAbilityGroup_ID { get; set; }
    public int GoldCost { get; set; }
    public int MaterialCost { get; set; }

    public override string ToString()
    {
        return $"TowerUpgradeAbilityUnlock_ID: {TowerUpgradeAbilityUnlock_ID}, AttackTower_ID: {AttackTower_ID}, RandomAbility_ID: {RandomAbility_ID}, RandomAbilityGroup_ID: {RandomAbilityGroup_ID}, GoldCost: {GoldCost}, MaterialCost: {MaterialCost}";
    }
}
public class TowerUpgradeAbilityUnlockTable : DataTable
{
    private readonly Dictionary<int, TowerUpgradeAbilityUnlockData> dictionary = new Dictionary<int, TowerUpgradeAbilityUnlockData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<TowerUpgradeAbilityUnlockData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.TowerUpgradeAbilityUnlock_ID, item))
            {
                Debug.LogError($"키 중복: {item.TowerUpgradeAbilityUnlock_ID}");
            }
        }
    }

    public TowerUpgradeAbilityUnlockData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public int GetDataId(int attackTowerId)
    {
        foreach (var item in dictionary.Values)
        {
            if (item.AttackTower_ID == attackTowerId)
            {
                return item.TowerUpgradeAbilityUnlock_ID;
            }
        }
        return -1;
    }
}