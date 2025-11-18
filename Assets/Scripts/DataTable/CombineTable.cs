using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CombineData
{
    public int Comb_Id { get; set; }
    public int Enemy_Id_1 { get; set; }
    public int EnemyQuantity_1 { get; set; }
    public int SpawnPoint_1 { get; set; }
    public int Enemy_Id_2 { get; set; }
    public int EnemyQuantity_2 { get; set; }
    public int SpawnPoint_2 { get; set; }
    public int Enemy_Id_3 { get; set; }
    public int EnemyQuantity_3 { get; set; }
    public int SpawnPoint_3 { get; set; }

}

public class CombineTable : DataTable
{
    private readonly Dictionary<int, CombineData> dictionary = new Dictionary<int, CombineData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<CombineData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Comb_Id, item))
            {
                Debug.LogError($"키 중복: {item.Comb_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public CombineData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
