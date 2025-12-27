using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
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
    public string RandomAbility2Name { get; set; }
    public int? SpecialEffect2_ID { get; set; }
    public float? SpecialEffect2Value { get; set; }
    public string RandomAbility3Name { get; set; }
    public int? SpecialEffect3_ID { get; set; }
    public float? SpecialEffect3Value { get; set; }
    public int RandomAbilityText_ID { get; set; }
    public int RandomAbilityType { get; set; }
    public float SuperSpecialEffectValue { get; set; }
    public string RandomAbilityReinforceUpgrade_ID { get; set; }
    
    [Ignore]
    public int[] RandomAbilityReinforceUpgrade_ID_Variable { get; set; }
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
            item.RandomAbilityReinforceUpgrade_ID_Variable =
                ParseBracketIntArray(item.RandomAbilityReinforceUpgrade_ID);

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

    public List<RandomAbilityData> GetAllAbilityDatas()
    {
        var allAbilities = new List<RandomAbilityData>();
        foreach (var ability in dictionary.Values)
        {
            if (ability.RandomAbility_ID == 200016 || ability.RandomAbility_ID == 200017)
                continue;

            allAbilities.Add(ability);
        }

        return allAbilities;
    }

    private static int[] ParseBracketIntArray(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<int>();

        raw = raw.Trim();

        if (raw.Length >= 2 && raw[0] == '"' && raw[^1] == '"')
            raw = raw.Substring(1, raw.Length - 2);

        raw = raw.Trim().TrimStart('[').TrimEnd(']');
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<int>();

        var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new int[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i].Trim(), out result[i]))
            {
                Debug.LogError($"RandomAbilityReinforceUpgrade_ID 파싱 실패: '{parts[i]}' (raw: '{raw}')");
                result[i] = 0;
            }
        }
        return result;
    }
}