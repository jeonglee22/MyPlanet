using System;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public static int Gold {get; set;} = 100000;
    public static int FreeDia {get; set;} = 2000;
    public static int ChargedDia {get; set;} = 3000;
    public static int TowerDictionaryRate {get; set;} = 0;
    public static int AbilityDictionaryRate {get; set;} = 0;
    public static int TowerEnhanceItem {get; set;} = 0;
    public static int PlanetEnhanceItem {get; set;} = 0;
    public static int HealthPlanetPiece {get; set;} = 0;
    public static int DefensePlanetPiece {get; set;} = 0;
    public static int ShieldPlanetPiece {get; set;} = 0;
    public static int BloodAbsorbPlanetPiece {get; set;} = 0;
    public static int ExpPlanetPiece {get; set;} = 0;
    public static int HealthRegenerationPlanetPiece {get; set;} = 0;
    public static int CommonPlanetPiece {get; set;} = 0;
}

public static class UserDataMapper
{
    private static Dictionary<int, (Func<int> getter, Action<int> setter, int maxStack)> itemMapping;

    static UserDataMapper()
    {
        itemMapping = new Dictionary<int, (Func<int>, Action<int>, int)>();

        RegisterCurrency(711101, () => UserData.Gold, (value) => UserData.Gold = value);
        RegisterCurrency(711201, () => UserData.FreeDia, (value) => UserData.FreeDia = value);
        RegisterCurrency(711202, () => UserData.ChargedDia, (value) => UserData.ChargedDia = value);

        RegisterItem(710101, () => UserData.TowerDictionaryRate, (value) => UserData.TowerDictionaryRate = value);
        RegisterItem(710102, () => UserData.AbilityDictionaryRate, (value) => UserData.AbilityDictionaryRate = value);

        RegisterItem(710201, () => UserData.TowerEnhanceItem, (value) => UserData.TowerEnhanceItem = value);
        RegisterItem(710202, () => UserData.PlanetEnhanceItem, (value) => UserData.PlanetEnhanceItem = value);

        RegisterItem(710301, () => UserData.HealthPlanetPiece, (value) => UserData.HealthPlanetPiece = value);
        RegisterItem(710302, () => UserData.DefensePlanetPiece, (value) => UserData.DefensePlanetPiece = value);
        RegisterItem(710303, () => UserData.ShieldPlanetPiece, (value) => UserData.ShieldPlanetPiece = value);
        RegisterItem(710304, () => UserData.BloodAbsorbPlanetPiece, (value) => UserData.BloodAbsorbPlanetPiece = value);
        RegisterItem(710305, () => UserData.ExpPlanetPiece, (value) => UserData.ExpPlanetPiece = value);
        RegisterItem(710306, () => UserData.HealthRegenerationPlanetPiece, (value) => UserData.HealthRegenerationPlanetPiece = value);
        RegisterItem(710307, () => UserData.CommonPlanetPiece, (value) => UserData.CommonPlanetPiece = value);
    }

    private static void RegisterItem(int itemId, Func<int> getter, Action<int> setter)
    {
        var itemData = DataTableManager.ItemTable.Get(itemId);
        itemMapping[itemId] = (getter, setter, itemData.MaxStack);
    }

    private static void RegisterCurrency(int currencyId, Func<int> getter, Action<int> setter)
    {
        var currencyData = DataTableManager.CurrencyTable.Get(currencyId);
        itemMapping[currencyId] = (getter, setter, currencyData.MaxStack);
    }

    public static int GetItemCount(int itemId)
    {
        if(itemMapping.TryGetValue(itemId, out var mapping))
        {
            return mapping.getter();
        }

        return 0;
    }

    public static void AddItem(int itemId, int amount)
    {
        if(itemMapping.TryGetValue(itemId, out var mapping))
        {
            int currentValue = mapping.getter();
            int newAmount = Mathf.Min(currentValue + amount, mapping.maxStack);
            mapping.setter(newAmount);
        }
    }

    public static int GetMaxCount(int itemId)
    {
        return itemMapping.TryGetValue(itemId, out var mapping) ? mapping.maxStack : 0;
    }
}

public static class PlanetPieceMapper
{
    private static Dictionary<int, int> planetPiece = new Dictionary<int, int>
    {
        { 300002, 710301 },
        { 300003, 710302 },
        { 300004, 710303 },
        { 300005, 710304 },
        { 300006, 710305 },
        { 300007, 710306 },
    };

    public static int GetPieceId(int planetId)
    {
        return planetPiece.TryGetValue(planetId, out var fragmentId) ? fragmentId : -1;
    }
}
