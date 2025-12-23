using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StageData
{
    public int Stage_Id { get; set; }
    public string StageName { get; set; }
    public string StageNameText { get; set; }
    public int StageIndex { get; set; }
    [TypeConverter(typeof(IntArrayConverter))]
    public int[] WaveGroup { get; set; }
    public int FirstReward_Id { get; set; }
    public int Reward_Id { get; set; }
    public int UnlockCondition { get; set; }
    public string BossName { get; set; }
    public string StageImage { get; set; }
    public string LockedStageName { get; set; }
    public string LockedStageImage { get; set; }

    public override string ToString()
    {
        return $"Stage_Id: {Stage_Id}, StageName: {StageName}, StageNameText: {StageNameText}, StageIndex: {StageIndex}, WaveGroup: [{string.Join(", ", WaveGroup)}], FirstReward_Id: [{string.Join(", ", FirstReward_Id)}], Reward_Id: [{string.Join(", ", Reward_Id)}], UnlockCondition: {UnlockCondition}";
    }
}

public class StageTable : DataTable
{
    private readonly Dictionary<int, StageData> dictionary = new Dictionary<int, StageData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<StageData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Stage_Id, item))
            {
                Debug.LogError($"키 중복: {item.Stage_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public StageData Get(int key)
    {
        if(!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }

    public int GetStageCount()
    {
        return dictionary.Count;
    }
}
