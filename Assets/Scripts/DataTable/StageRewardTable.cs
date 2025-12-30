using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StageRewardData
{
    public int StageReward_Id { get; set; }
    public int StageRewardType { get; set; }
    public int Target_Id_1 { get; set; }
    public int RewardQty_1 { get; set; }
    public int Target_Id_2 { get; set; }
    public int RewardQty_2 { get; set; }
    public int Target_Id_3 { get; set; }
    public int RewardQty_3 { get; set; }
    public int Target_Id_4 { get; set; }
    public int RewardQty_4 { get; set; }
    public int Target_Id_5 { get; set; }
    public int RewardQty_5 { get; set; }
}

public class StageRewardTable : DataTable
{
    private readonly Dictionary<int, StageRewardData> dictionary = new Dictionary<int, StageRewardData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<StageRewardData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.StageReward_Id, item))
            {
                Debug.LogError($"키 중복: {item.StageReward_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public StageRewardData Get(int key)
    {
        if(!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }

    public int GetRewardCount(int stageRewardId)
    {
        if (!dictionary.ContainsKey(stageRewardId))
        {
            Debug.LogError($"키 없음: {stageRewardId}");
            return 0;
        }

        var rewardData = dictionary[stageRewardId];
        int count = 0;

        if (rewardData.Target_Id_1 > 0)
        {
            count++;
        } 
        if (rewardData.Target_Id_2 > 0)
        {
            count++;
        }
        if (rewardData.Target_Id_3 > 0)
        {
            count++;
        }
        if( rewardData.Target_Id_4 > 0)
        {
            count++;
        }
        if( rewardData.Target_Id_5 > 0)
        {
            count++;
        }

        return count;
    }
}
