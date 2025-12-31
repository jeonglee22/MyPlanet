using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlanetData
{
    public int Planet_ID { get; set; }
    public string PlanetName { get; set; }
    public int PlanetGrade { get; set; }
    public int PlanetHp { get; set; }
    public int PlanetArmor { get; set; }
    public int PlanetShield { get; set; }
    public int RotateSpeed { get; set; }
    public int BaseOrbitSpeed { get; set; }
    public int CtrlOrbitSpeed { get; set; }
    public int SlotNum { get; set; }
    public int MaxTowerNum { get; set; }
    public int Drain { get; set; }
    public float ExpScale { get; set; }
    public float RecoveryHp { get; set; }
    public float DrainChance { get; set; }
    public float BossDrain { get; set; }
    public int PlanetText_ID { get; set; }
    public int PieceId { get; set; }
    public string PlanetImage { get; set; }
    public string PlanetIcon { get; set; }
    public string Warplanet { get; set; }

    public override string ToString()
    {
        return $"Planet_ID: {Planet_ID}, PlanetName: {PlanetName}, PlanetGrade: {PlanetGrade}, PlanetHp: {PlanetHp}, PlanetArmor: {PlanetArmor}, PlanetShield: {PlanetShield}, RotateSpeed: {RotateSpeed}, BaseOrbitSpeed: {BaseOrbitSpeed}, CtrlOrbitSpeed: {CtrlOrbitSpeed}, SlotNum: {SlotNum}, MaxTowerNum: {MaxTowerNum}, Drain: {Drain}, ExpScale: {ExpScale}, RecoveryHp: {RecoveryHp}";
    }

}

public class PlanetTable : DataTable
{
    private readonly Dictionary<int, PlanetData> dictionary = new Dictionary<int, PlanetData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<PlanetData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Planet_ID, item))
            {
                Debug.LogError($"키 중복: {item.Planet_ID}");
            }
        }
    }

    public PlanetData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public List<PlanetData> GetAll()
    {
        return new List<PlanetData>(dictionary.Values);
    }
}
