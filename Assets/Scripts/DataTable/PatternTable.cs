using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PatternData
{
    public int Pattern_Id { get; set; }
    public string PatternName { get; set; }
    public int PatternGroup { get; set; }
    public int PatternType { get; set; }
    public int PatternTrigger { get; set; }
    public int PatternValue { get; set; }
    public float PatternDamageRate { get; set; }
    public int PatternList { get; set; }
    public float Weight { get; set; }
    public float Cooltime { get; set; }
    public float PatternDelay { get; set; }
    public float RepeatDelay { get; set; }
    public int MinionSpawn_Id { get; set; }
    public int Skill_Id { get; set; }

    public override string ToString()
    {
        return $"PatternId: {Pattern_Id}, PatternName: {PatternName}, PatternList: {PatternList}, PatternTrigger: {PatternTrigger}, PatternValue: {PatternValue}, PatternDamageRate: {PatternDamageRate}, Weight: {Weight}, Cooltime: {Cooltime}";   
    }
}

public class PatternTable : DataTable
{
    private readonly Dictionary<int, PatternData> dictionary = new Dictionary<int, PatternData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<PatternData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Pattern_Id, item))
            {
                Debug.LogError($"키 중복: {item.Pattern_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public PatternData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public List<PatternData> GetPatternList(int patternGroup)
    {
        List<PatternData> patterns = new List<PatternData>();

        foreach (var pattern in dictionary.Values)
        {
            if (pattern.PatternGroup == patternGroup)
            {
                patterns.Add(pattern);
            }
        }

        return patterns;
    }
}
