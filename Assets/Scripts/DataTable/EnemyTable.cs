using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyTableData
{
    public int Enemy_Id { get; set; }
    public string EnemyName { get; set; }
    public string EnemyTextName { get; set; }
    public int EnemyType { get; set; }
    public int EnemyGrade { get; set; }
    public int AttackType { get; set; }
    public int MoveType { get; set; }
    public float Hp { get; set; }
    public float Defense { get; set; }
    public float Shield { get; set; }
    public float MoveSpeed { get; set; }
    public float Attack { get; set; }
    public float UniqueRatePenetration { get; set; }
    public float FixedPenetration { get; set; }
    public float Exp { get; set; }
    public string VisualAsset { get; set; }
    public int PatternGroup { get; set; }


    public override string ToString()
    {
        return $"Enemy_Id: {Enemy_Id}, Name: {EnemyName}, EnemyType: {EnemyType}, EnemyGrade: {EnemyGrade}, AttackType: {AttackType}, MoveType: {MoveType}, Hp: {Hp}, Defense: {Defense}, Shield: {Shield}, MoveSpeed: {MoveSpeed}, Attack: {Attack}, UniqueRatePenetration: {UniqueRatePenetration}, FixedPenetration: {FixedPenetration}, Exp: {Exp}";
    }
}

public class EnemyTable : DataTable
{
    private readonly Dictionary<int, EnemyTableData> dictionary = new Dictionary<int, EnemyTableData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<EnemyTableData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Enemy_Id, item))
            {
                Debug.LogError($"키 중복: {item.Enemy_Id}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public EnemyTableData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public List<int> GetEnemyIds()
    {
        return new List<int>(dictionary.Keys);
    }
}
