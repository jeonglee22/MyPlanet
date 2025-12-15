using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MinionSpawnData
{
    public int MinionSpawn_Id { get; set; }
    public int Enemy_Id { get; set; }
    public int EnemyQuantity_1 { get; set; }
    public int Skill_Id { get; set; }

    public override string ToString()
    {
        return $"MinionSpawn_Id: {MinionSpawn_Id}, Enemy_Id: {Enemy_Id}, EnemyQuantity_1: {EnemyQuantity_1}";
    }
}

public class MinionSpawnTable : DataTable
{
    private readonly Dictionary<int, MinionSpawnData> dictionary = new Dictionary<int, MinionSpawnData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<MinionSpawnData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.MinionSpawn_Id, item))
            {
                Debug.LogError($"키 중복: {item.MinionSpawn_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public MinionSpawnData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
