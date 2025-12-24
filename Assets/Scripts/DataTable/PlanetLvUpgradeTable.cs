using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlanetLvUpgradeData
{
    public int PlanetLvUpgrade_ID { get; set; }
    public int PlanetLvUpgradeLevel { get; set; }
    public float AddHp { get; set; }
    public float AddArmor { get; set; }
    public int UpgradeResource { get; set; }
    public int Gold { get; set; }
    public int Planet_ID { get; set; }

}

public class PlanetLvUpgradeTable : DataTable
{
    private readonly Dictionary<int, PlanetLvUpgradeData> dictionary = new Dictionary<int, PlanetLvUpgradeData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<PlanetLvUpgradeData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.PlanetLvUpgrade_ID, item))
            {
                Debug.LogError($"키 중복: {item.PlanetLvUpgrade_ID}");
            }
        }
    }

    public PlanetLvUpgradeData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public PlanetLvUpgradeData GetCurrentLevelData(int planetId, int currentLevel)
    {
        foreach (var data in dictionary.Values)
        {
            if (data.Planet_ID == planetId && data.PlanetLvUpgradeLevel == currentLevel)
            {
                return data;
            }
        }

        return null;
    }
}
