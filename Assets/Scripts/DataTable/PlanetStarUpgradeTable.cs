using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlanetStarUpgradeData
{
    public int PlanetStarUpgrade_ID { get; set; }
    public int PlanetStarUpgradeLevel { get; set; }
    public int PlanetAbilityType { get; set; }
    public float PlanetAbilityValue { get; set; }
    public int UpgradeResource { get; set; }
    public int Planet_ID { get; set; }

}

public class PlanetStarUpgradeTable : DataTable
{
    private readonly Dictionary<int, PlanetStarUpgradeData> dictionary = new Dictionary<int, PlanetStarUpgradeData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<PlanetStarUpgradeData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.PlanetStarUpgrade_ID, item))
            {
                Debug.LogError($"키 중복: {item.PlanetStarUpgrade_ID}");
            }
        }
    }

    public PlanetStarUpgradeData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public PlanetStarUpgradeData GetCurrentLevelData(int planetId, int currentStarLevel)
    {
        foreach (var data in dictionary.Values)
        {
            if (data.Planet_ID == planetId && data.PlanetStarUpgradeLevel == currentStarLevel)
            {
                return data;
            }
        }

        return null;
    }
}
