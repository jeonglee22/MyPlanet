using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CurrencyData
{
    public int Currency_Id { get; set; }
    public int CurrencyType { get; set; }
    public int CurrencyGroup { get; set; }
    public string GroupName { get; set; }
    public string GroupNameText { get; set; }
    public int Priority { get; set; }
    public int MaxStack { get; set; }
    public string CurrencyName { get; set; }
    public string CurrencyNameText { get; set; }
    public string CurrencyDescription { get; set; }

    public override string ToString()
    {
        return $"Currency_Id: {Currency_Id}, Name: {CurrencyName}, CurrentType: {CurrencyType}, CurrencyGroup: {CurrencyGroup}, Priority: {Priority}, MaxStack: {MaxStack}, Description: {CurrencyDescription}";
    }
}

public class CurrencyTable : DataTable
{
    private readonly Dictionary<int, CurrencyData> dictionary = new Dictionary<int, CurrencyData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<CurrencyData>(textAsset.text);
        foreach (var currency in list)
        {
            if (!dictionary.TryAdd(currency.Currency_Id, currency))
            {
                Debug.LogError($"키 중복: {currency.Currency_Id}");
            }
        }

        /* test : data table load check
        foreach(var currency in list)
        {
            Debug.Log(currency.ToString());
        }
        */
    }

    public CurrencyData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            Debug.LogError($"키 없음: {key}");
            return null;
        }

        return dictionary[key];
    }

    public List<CurrencyData> GetAll()
    {
        return new List<CurrencyData>(dictionary.Values);
    }
}
