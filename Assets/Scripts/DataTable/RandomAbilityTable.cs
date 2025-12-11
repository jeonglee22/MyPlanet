using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RandomAbilityData
{
    public int RandomAbility_ID { get; set; }
    public string RandomAbilityName { get; set; }
    public int SpecialEffect_ID { get; set; }
    public float SpecialEffectValue { get; set; }
    public float Weight { get; set; }
    public int PlaceType { get; set; }
    public int RandonSlotNum { get; set; }
    public int AddSlotNum { get; set; }
    public int DuplicateType { get; set; }

}

public class RandomAbilityTable : DataTable
{
    private readonly Dictionary<int, RandomAbilityData> dictionary = new Dictionary<int, RandomAbilityData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<RandomAbilityData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.RandomAbility_ID, item))
            {
                Debug.LogError($"키 중복: {item.RandomAbility_ID}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public RandomAbilityData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public int GetAbilityIdFromEffectId(int effectId)
    {
        var values = dictionary.Values;
        foreach (var data in values)
        {
            if (data.SpecialEffect_ID == effectId)
                return data.RandomAbility_ID;
        }

        return -1;
    }

    public List<RandomAbilityData> GetAllAbilityIds()
    {
        return new List<RandomAbilityData>(dictionary.Values);
    }
}