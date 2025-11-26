using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlanetLevelUpData
{
    public int PlanetLevel { get; set; }
    public float Exp { get; set; }
}

public class PlanetLevelUpTable : DataTable
{
    private readonly Dictionary<int, PlanetLevelUpData> dictionary = new Dictionary<int, PlanetLevelUpData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<PlanetLevelUpData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.PlanetLevel, item))
            {
                Debug.LogError($"키 중복: {item.PlanetLevel}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public PlanetLevelUpData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
