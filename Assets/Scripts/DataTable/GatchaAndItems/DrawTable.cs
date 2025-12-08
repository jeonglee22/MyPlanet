using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DrawData
{
    public int Draw_Id { get; set; }
    public int DrawGroup { get; set; }
    public int DrawType { get; set; }
    public string DrawTypeText { get; set; }
    public int CurrencyGroup { get; set; }
    public int CostPolicy { get; set; }
    public int NeedCurrencyValue { get; set; }
    public int Reward_Id { get; set; }
    public string RewardName { get; set; }
    public int RewardQty { get; set; }
    public float Weight { get; set; }
    public float ActualRate { get; set; }
    public string DisplayedRate { get; set; }

    public override string ToString()
    {
        return $"Draw_Id: {Draw_Id}, DrawType: {DrawType}, CurrencyGroup: {CurrencyGroup}, NeedCurrencyValue: {NeedCurrencyValue}, Reward_Id: {Reward_Id}, RewardQty: {RewardQty}, ActualRate: {ActualRate}, DisplayedRate: {DisplayedRate}";
    }
}

public class DrawTable : DataTable
{
    private readonly Dictionary<int, DrawData> dictionary = new Dictionary<int, DrawData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<DrawData>(textAsset.text);
        foreach (var draw in list)
        {
            if (!dictionary.TryAdd(draw.Draw_Id, draw))
            {
                Debug.LogError($"키 중복: {draw.Draw_Id}");
            }
        }

        /* test : data table load check
        foreach(var draw in list)
        {
            Debug.Log(draw.ToString());
        }
        */
    }

    public DrawData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }

    public List<(int, string)> GetGachaList()
    {
        List<(int, string)> gachaList = new List<(int, string)>();
        int currentGroup = -1;
        foreach (var draw in dictionary.Values)
        {
            if (draw.DrawGroup != currentGroup)
            {
                currentGroup = draw.DrawGroup;
                gachaList.Add((currentGroup, draw.DrawTypeText));
            }
        }

        return gachaList;
    }
}
