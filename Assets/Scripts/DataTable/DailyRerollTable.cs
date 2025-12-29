using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DailyRerollData
{
    public int DailyReroll_Id { get; set; }
    public int CurrencyGroup { get; set; }
    public int NeedCurrencyValue { get; set; }
    public int Reward_Id { get; set; }
    public int Weight { get; set; }
    public int WeightType { get; set; }
    public float ActualRate { get; set; }
    public string DisplayedRate { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }

    public override string ToString()
    {
        return $"DailyReroll_Id: {DailyReroll_Id}, CurrencyGroup: {CurrencyGroup}, NeedCurrencyValue: {NeedCurrencyValue}, Reward_Id: {Reward_Id}, Weight: {Weight}, WeightType: {WeightType}, ActualRate: {ActualRate}, DisplayedRate: {DisplayedRate}, MinQuantity: {MinQuantity}, MaxQuantity: {MaxQuantity}";
    }
}

public class DailyRerollTable : DataTable
{
    private readonly Dictionary<int, DailyRerollData> dictionary = new Dictionary<int, DailyRerollData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<DailyRerollData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.DailyReroll_Id, item))
            {
                Debug.LogError($"키 중복: {item.DailyReroll_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public DailyRerollData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public DailyRerollData GetFromRewardId(int rewardId)
    {
        foreach (var data in dictionary.Values)
        {
            if (data.Reward_Id == rewardId)
            {
                return data;
            }
        }

        return null;
    }

    public DailyRerollData GetRandomData()
    {
        if (dictionary.Count == 0)
        {
            return null;
        }

        var keys = new List<int>(dictionary.Keys);
        var randomKey = keys[Random.Range(0, keys.Count)];
        return dictionary[randomKey];
    }

    public DailyRerollData GetRandomDataExceptKeys(List<int> excludeKeys)
    {
        var availableData = new List<DailyRerollData>();
        foreach (var data in dictionary.Values)
        {
            if (!excludeKeys.Contains(data.DailyReroll_Id))
            {
                availableData.Add(data);
            }
        }

        if (availableData.Count == 0)
        {
            return null;
        }

        var randomIndex = Random.Range(0, availableData.Count);
        return availableData[randomIndex];
    }

    public int GetRandomCountInId(int key)
    {
        var data = Get(key);
        if (data == null)
        {
            return 0;
        }

        return Random.Range(data.MinQuantity, data.MaxQuantity + 1);
    }
}