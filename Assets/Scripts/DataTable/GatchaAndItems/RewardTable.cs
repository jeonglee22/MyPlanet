using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RewardData
{
    public int Reward_Id { get; set; }
    public int RewardType { get; set; }
    public int Target_Id { get; set; }
    public string RewardName { get; set; }
    public string RewardNameText { get; set; }
    public int Stack { get; set; }
    public int Rarity { get; set; }

    public override string ToString()
    {
        return $"Reward_Id: {Reward_Id}, Name: {RewardName}, RewardType: {RewardType}, Target_Id: {Target_Id}, Stack: {Stack}, Rarity: {Rarity}";
    }
}

public class RewardTable : DataTable
{
    private readonly Dictionary<int, RewardData> dictionary = new Dictionary<int, RewardData>();
    public Dictionary<int, RewardData> Dictionary => dictionary;

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<RewardData>(textAsset.text);
        foreach (var reward in list)
        {
            if (!dictionary.TryAdd(reward.Reward_Id, reward))
            {
                Debug.LogError($"키 중복: {reward.Reward_Id}");
            }
        }

        /* test : data table load check
        foreach(var reward in list)
        {
            Debug.Log(reward.ToString());
        }
        */
    }

    public RewardData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }

    public RewardData GetFromTargetId(int targetId)
    {
        foreach (var reward in dictionary.Values)
        {
            if (reward.Target_Id == targetId)
            {
                return reward;
            }
        }

        return null;
    }
}
