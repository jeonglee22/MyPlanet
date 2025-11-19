using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProjectileTableData
{
    public int Projectile_ID { get; set; }
    public string ProjectileName { get; set; }
    public float AttackType { get; set; }
    public float TargetNum { get; set; }
    public float HitType { get; set; }
    public float CollisionSize { get; set; }
    public float Attack { get; set; }
    public float FixedPenetration { get; set; }
    public float RatePenetration { get; set; }
    public float ProjectileSpeed { get; set; }
    public float ProjectileAddSpeed { get; set; }
    public float RemainTime { get; set; }
    public int ProjectileProperties1_ID { get; set; }
    public float ProjectileProperties1Value { get; set; }
    public int ProjectileProperties2_ID { get; set; }
    public float ProjectileProperties2Value { get; set; }
    public int ProjectileProperties3_ID { get; set; }
    public float ProjectileProperties3Value { get; set; }

    public override string ToString()
    {
        return $"{Projectile_ID}, {ProjectileName}, {AttackType}, {TargetNum}, {HitType}, {CollisionSize}, {Attack}, {FixedPenetration}, {RatePenetration}, {ProjectileSpeed}, {ProjectileAddSpeed}, {RemainTime}, {ProjectileProperties1_ID}, {ProjectileProperties1Value}, {ProjectileProperties2_ID}, {ProjectileProperties2Value}, {ProjectileProperties3_ID}, {ProjectileProperties3Value}";
    }
}

public class ProjectileTable : DataTable
{
    private readonly Dictionary<int, ProjectileTableData> dictionary = new Dictionary<int, ProjectileTableData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<ProjectileTableData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Projectile_ID, item))
            {
                Debug.LogError($"키 중복: {item.Projectile_ID}");
            }
        }

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
    }

    public ProjectileTableData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }
}
