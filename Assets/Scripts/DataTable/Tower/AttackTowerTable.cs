using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AttackTowerTableRow
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
    public float TowerWeight { get; set; }
    public int Order { get; set; }
    public int TowerText_ID { get; set; }
    public string AttackTowerAsset { get; set; }
    public string AttackTowerAssetCut { get; set; }

    [Name("TowerReinforceUpgrade_ID")]
    public string TowerReinforceUpgrade_ID_Raw { get; set; }

    // parsing
    [System.NonSerialized]
    private int[] towerReinforceUpgradeIds;

    [Ignore]
    public int[] TowerReinforceUpgrade_ID
    {
        get
        {
            if (towerReinforceUpgradeIds != null)
                return towerReinforceUpgradeIds;

            towerReinforceUpgradeIds = ParseIdArray(TowerReinforceUpgrade_ID_Raw);
            return towerReinforceUpgradeIds;
        }
    }

    private int[] ParseIdArray(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<int>();

        raw = raw.Trim();
        if (raw.StartsWith("["))
            raw = raw.Substring(1);
        if (raw.EndsWith("]"))
            raw = raw.Substring(0, raw.Length - 1);

        var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<int>();

        foreach (var p in parts)
        {
            if (int.TryParse(p.Trim(), out int id))
                result.Add(id);
        }

        return result.ToArray();
    }
}

public class AttackTowerTable : DataTable
{
    public List<AttackTowerTableRow> Rows { get; private set; } = new List<AttackTowerTableRow>();
    private readonly Dictionary<int, AttackTowerTableRow> rowById = new Dictionary<int, AttackTowerTableRow>();

    public override async UniTask LoadAsync(string filename)
    {
        Rows.Clear();
        rowById.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<AttackTowerTableRow>(textAsset.text);

        Rows.AddRange(list);

        foreach (var row in list)
        {
            if (!rowById.TryAdd(row.AttackTower_Id, row))
            {
                Debug.LogError($"[AttackTowerTable] Duplicate Id: {row.AttackTower_Id}");
            }
        }
    }

    public AttackTowerTableRow GetById(int id)
    {
        if (rowById.TryGetValue(id, out var row))
        {
            return row;
        }
        return null;
    }

    public int GetTowerTextIdById(int id)
    {
        var row = GetById(id);
        if (row != null)
        {
            return row.TowerText_ID;
        }
        return -1;
    }

    public List<AttackTowerTableRow> GetAllDatas()
    {
        List<AttackTowerTableRow> result = new List<AttackTowerTableRow>();
        result = Rows;

        result.Sort((a, b) => a.Order.CompareTo(b.Order));
        return Rows;
    }

    public async UniTask SaveOverridesAsync()
    {
        var changedRows = new List<AttackTowerTableRow>();
        foreach (var kvp in rowById)
        {
            var attackTower = kvp.Value;

            changedRows.Add(attackTower);
        }

        var csvText = CsvSaveUtil.ToCsv(changedRows);
        var path = Path.Combine("Assets/DataTables/", "attackTowerTable.csv");
        await CsvSaveUtil.SaveTextAsync(path, csvText);
        Debug.Log($"Saved override: {path}");
    }

    public void Set(int key, AttackTowerTableRow data)
    {
        if (rowById.ContainsKey(key))
        {
            rowById[key] = data;
        }
    }
}
