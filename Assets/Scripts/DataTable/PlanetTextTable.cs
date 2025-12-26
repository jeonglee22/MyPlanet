using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlanetTextData
{
    public int PlanetText_ID { get; set; }
    public string PlanetName { get; set; }
    public string PlanetDescribe { get; set; }

}

public class PlanetTextTable : DataTable
{
    private readonly Dictionary<int, PlanetTextData> dictionary = new Dictionary<int, PlanetTextData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<PlanetTextData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.PlanetText_ID, item))
            {
                Debug.LogError($"키 중복: {item.PlanetText_ID}");
            }
        }
    }

    public PlanetTextData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
