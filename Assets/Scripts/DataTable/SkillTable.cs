using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SkillData
{
    public int Skill_Id { get; set; }
    public string Dev_SkillName { get; set; }
    public int RangeValue { get; set; }
    public int ProjectileQty { get; set; }
    public float ProjectileTerm { get; set; }
    public int Targeting { get; set; }
    public int EffectStart { get; set; }
    public int EffectDirection { get; set; }
    public int EffectArrive { get; set; }
    public int EffectEnd { get; set; }
    public float Duration { get; set; }
    public float RepeatTerm { get; set; }
    public int RepeatCount { get; set; }
    public int Stackable { get; set; }

    public override string ToString()
    {
        return $"Skill_Id: {Skill_Id}, Dev_SkillName: {Dev_SkillName}, RangeValue: {RangeValue}, ProjectileQty: {ProjectileQty}, ProjectileTerm: {ProjectileTerm}, Targeting: {Targeting}, EffectStart: {EffectStart}, EffectDirection: {EffectDirection}, EffectArrive: {EffectArrive}, EffectEnd: {EffectEnd}, Duration: {Duration}, RepeatTerm: {RepeatTerm}, RepeatCount: {RepeatCount}, Stackable: {Stackable}";
    }
}

public class SkillTable : DataTable
{
    private readonly Dictionary<int, SkillData> dictionary = new Dictionary<int, SkillData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<SkillData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Skill_Id, item))
            {
                Debug.LogError($"키 중복: {item.Skill_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public SkillData Get(int key)
    {
        if(!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }
}
