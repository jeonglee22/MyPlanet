using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class SpecialEffectData
{
    public int SpecialEffect_ID { get; set; }
    public int SpecialEffectType { get; set; }
    public string SpecialEffectName { get; set; }
    public int SpecialEffectValueType { get; set; }
    public string SpecialEffectFile { get; set; }
    public float SpecialEffectAbility { get; set; }
    public string SpecialEffectIcon { get; set; }

    public override string ToString()
    {
        return $"{SpecialEffect_ID}, Type={SpecialEffectType}, " +
               $"{SpecialEffectName}, ValueType={SpecialEffectValueType}, File={SpecialEffectFile}";
    }
}

public class SpecialEffectTable : DataTable
{
    private readonly Dictionary<int, SpecialEffectData> dictionary =
        new Dictionary<int, SpecialEffectData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<SpecialEffectData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.SpecialEffect_ID, item))
            {
                Debug.LogError($"[SpecialEffectTable] �ߺ� Ű: {item.SpecialEffect_ID}");
            }
        }
    }

    public SpecialEffectData Get(int specialEffectId)
    {
        if (!dictionary.TryGetValue(specialEffectId, out var data))
        {
            return null;
        }
        return data;
    }
    public IReadOnlyDictionary<int, SpecialEffectData> GetAll() => dictionary;
}