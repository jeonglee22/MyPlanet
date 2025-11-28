using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerReinforceUpgradeRow
{
    public int TowerReinforceUpgrade_ID { get; set; }        
    public int ReinforceUpgradeLevel { get; set; }     
    public int AddValue { get; set; }                
}

public class TowerReinforceUpgradeTable : DataTable
{
    public List<TowerReinforceUpgradeRow> Rows { get; private set; } = new List<TowerReinforceUpgradeRow>();
    private readonly Dictionary<int, TowerReinforceUpgradeRow> rowById = new Dictionary<int, TowerReinforceUpgradeRow>();

    public override async UniTask LoadAsync(string filename)
    {
        Rows.Clear();
        rowById.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<TowerReinforceUpgradeRow>(textAsset.text);

        Rows.AddRange(list);

        foreach (var row in list)
        {
            if (!rowById.TryAdd(row.TowerReinforceUpgrade_ID, row))
            {
                Debug.LogError($"[TowerReinforceUpgradeTable] Duplicate Id: {row.TowerReinforceUpgrade_ID}");
            }
        }
    }

    public TowerReinforceUpgradeRow GetById(int id)
    {
        if (rowById.TryGetValue(id, out var row))
        {
            return row;
        }
        return null;
    }
}
