using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BuffTowerReinforceUpgradeRow
{
    public int BuffTowerReinforceUpgrade_ID { get; set; }                
    public int ReinforceUpgradeLevel { get; set; }         
    public int SpecialEffect1_ID { get; set; }          
    public float SpecialEffect1AddValue { get; set; }         
    public int SpecialEffect2_ID { get; set; }           
    public float SpecialEffect2AddValue { get; set; }       
    public int SpecialEffect3_ID { get; set; }       
    public float SpecialEffect3AddValue { get; set; }
}

public class BuffTowerReinforceUpgradeTable : DataTable
{
    public List<BuffTowerReinforceUpgradeRow> Rows { get; private set; } = new List<BuffTowerReinforceUpgradeRow>();
    private readonly Dictionary<int, BuffTowerReinforceUpgradeRow> rowById = new Dictionary<int, BuffTowerReinforceUpgradeRow>();

    public override async UniTask LoadAsync(string filename)
    {
        Rows.Clear();
        rowById.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<BuffTowerReinforceUpgradeRow>(textAsset.text);

        Rows.AddRange(list);

        foreach (var row in list)
        {
            if (!rowById.TryAdd(row.BuffTowerReinforceUpgrade_ID, row))
            {
                Debug.LogError($"[BuffTowerReinforceUpgradeTable] Duplicate Id: {row.BuffTowerReinforceUpgrade_ID}");
            }
        }
    }

    public BuffTowerReinforceUpgradeRow GetById(int id)
    {
        if (rowById.TryGetValue(id, out var row))
        {
            return row;
        }
        return null;
    }

    public BuffTowerReinforceUpgradeRow GetUpgradeDataWithTowerLevel(int id, int towerLevel)
    {
        foreach(var row in Rows)
        {
            if(row.BuffTowerReinforceUpgrade_ID == id && row.ReinforceUpgradeLevel == towerLevel)
            {
                return row;
            }
        }
        
        return null;
    }
}
