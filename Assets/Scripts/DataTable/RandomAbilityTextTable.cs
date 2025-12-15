using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RandomAbilityTextData
{
    public int RandomAbilityText_ID { get; set; }
    public string RandomAbilityName { get; set; }
    public string RandomAbilityDescribe { get; set; }
}

public class RandomAbilityTextTable : DataTable
{
    private readonly Dictionary<int, RandomAbilityTextData> dictionary = new Dictionary<int, RandomAbilityTextData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<RandomAbilityTextData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.RandomAbilityText_ID, item))
            {
                Debug.LogError($"키 중복: {item.RandomAbilityText_ID}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public RandomAbilityTextData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
