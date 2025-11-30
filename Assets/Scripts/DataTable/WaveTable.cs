using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WaveData
{
    public int Wave_Id { get; set; }
    public int WaveGroup { get; set; }
    public int WaveIndex { get; set; }
    public int StageIndex { get; set; }
    public int Comb_Id { get; set; }
    public float HpScale { get; set; }
    public float AttScale { get; set; }
    public float DefScale { get; set; }
    public float PenetScale { get; set; }
    public float MoveSpeedScale { get; set; }
    public float PrefabScale { get; set; }
    public float ExpScale { get; set; }
    public float SpawnTerm { get; set; }
    public int RepeatCount { get; set; }

    public override string ToString()
    {
        return $"{Wave_Id}, {WaveGroup}, {WaveIndex}, {StageIndex}, {Comb_Id}, {HpScale}, {AttScale}, {DefScale}, {PenetScale}, {MoveSpeedScale}, {PrefabScale}, {ExpScale}, {SpawnTerm}, {RepeatCount}";
    }
}

public class WaveTable : DataTable
{
    private readonly Dictionary<int, WaveData> dictionary = new Dictionary<int, WaveData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<WaveData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Wave_Id, item))
            {
                Debug.LogError($"키 중복: {item.Wave_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public WaveData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public List<WaveData> GetCurrentStageWaveData(int stageIndex)
    {
        List<WaveData> result = new List<WaveData>();
        foreach (var item in dictionary.Values)
        {
            if (item.StageIndex == stageIndex)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public int GetStageCount()
    {
        HashSet<int> stageIndices = new HashSet<int>();
        foreach (var item in dictionary.Values)
        {
            stageIndices.Add(item.StageIndex);
        }
        return stageIndices.Count;
    }
}
