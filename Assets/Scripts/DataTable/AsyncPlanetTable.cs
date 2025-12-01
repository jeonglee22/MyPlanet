using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AsyncPlanetData
{
    public int Asynch_Id { get; set; }
    public int AttackTower_Id { get; set; }
    public int TowerType { get; set; }
    public int Effect_Id_1 { get; set; }
    public int Effect_Id_2 { get; set; }
    public int Effect_Id_3 { get; set; }
    public int Index { get; set; }

    public override string ToString()
    {
        return $"Asynch_Id: {Asynch_Id}, AttackTower_Id: {AttackTower_Id}, TowerType: {TowerType}, Effect_Id_1: {Effect_Id_1}, Effect_Id_2: {Effect_Id_2}, Effect_Id_3: {Effect_Id_3}, Index: {Index}";
    }
}

public class AsyncPlanetTable : DataTable
{
    private readonly Dictionary<int, AsyncPlanetData> dictionary = new Dictionary<int, AsyncPlanetData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<AsyncPlanetData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Asynch_Id, item))
            {
                Debug.LogError($"키 중복: {item.Asynch_Id}");
            }
        }
    }

    public AsyncPlanetData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
