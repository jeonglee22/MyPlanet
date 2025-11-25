using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AttackTowerRow
{
    public int AttackTower_Id { get; set; }        
    public string AttackTowerName { get; set; }     
    public float FireType { get; set; }                
    public float AttackSpeed { get; set; }         
    public float AttackRange { get; set; }          
    public float Accuracy { get; set; }         
    public float grouping { get; set; }           
    public float ProjectileNum { get; set; }       
    public int Projectile_ID { get; set; }       
    public int RandomAbilityGroup_ID { get; set; }
}

public class AttackTowerTable : DataTable
{
    public List<AttackTowerRow> Rows { get; private set; } = new List<AttackTowerRow>();
    private readonly Dictionary<int, AttackTowerRow> rowById = new Dictionary<int, AttackTowerRow>();

    public override async UniTask LoadAsync(string filename)
    {
        Rows.Clear();
        rowById.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<AttackTowerRow>(textAsset.text);

        Rows.AddRange(list);

        foreach (var row in list)
        {
            if (!rowById.TryAdd(row.AttackTower_Id, row))
            {
                Debug.LogError($"[AttackTowerTable] Duplicate Id: {row.AttackTower_Id}");
            }
        }
        Debug.Log($"[AttackTowerTable] Loaded {Rows.Count} rows.");
    }

    public AttackTowerRow GetById(int id)
    {
        if (rowById.TryGetValue(id, out var row))
        {
            return row;
        }
        return null;
    }
}
