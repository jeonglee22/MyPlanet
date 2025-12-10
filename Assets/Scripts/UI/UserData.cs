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

    public static bool isHealthPlanet = false;
    public static bool isDefensePlanet = false;
    public static bool isShieldPlanet = false;
    public static bool isBloodAbsorbPlanet = false;
    public static bool isExpPlanet = false;
    public static bool isHealthRegenerationPlanet = false;

    public static int CollectionTowerCore = 12;
    public static int CollectionRandomAbilityCore = 10;
}

public static class UserDataMapper
{
    private static Dictionary<int, (Func<int> getter, Action<int> setter, int maxStack)> itemMapping;
    private static Dictionary<int, (Func<bool> getter, Action<bool> setter, int pieceId)> planetMapping;

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

        planetMapping = new Dictionary<int, (Func<bool>, Action<bool>, int)>
        {
            { 300002, (() => UserData.isHealthPlanet, (value) => UserData.isHealthPlanet = value, 710301) },
            { 300003, (() => UserData.isDefensePlanet, (value) => UserData.isDefensePlanet = value, 710302) },
            { 300004, (() => UserData.isShieldPlanet, (value) => UserData.isShieldPlanet = value, 710303) },
            { 300005, (() => UserData.isBloodAbsorbPlanet, (value) => UserData.isBloodAbsorbPlanet = value, 710304) },
            { 300006, (() => UserData.isExpPlanet, (value) => UserData.isExpPlanet = value, 710305) },
            { 300007, (() => UserData.isHealthRegenerationPlanet, (value) => UserData.isHealthRegenerationPlanet = value, 710306) },
        };
    }

    private static void RegisterItem(int itemId, Func<int> getter, Action<int> setter)
    {
        var itemData = DataTableManager.ItemTable.Get(itemId);
        itemMapping[itemId] = (getter, setter, itemData.MaxStack);
    }

    private static void RegisterCurrency(int currencyId, Func<int> getter, Action<int> setter)
    {
        var currencyData = DataTableManager.CurrencyTable.Get(currencyId);
        itemMapping[currencyId] = (
            getter, 
            (value) => setter(value) , 
            currencyData.MaxStack);
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

    public static bool HasPlanet(int planetId)
    {
        if(planetMapping.TryGetValue(planetId, out var mapping))
        {
            return mapping.getter();
        }

        return false;
    }

    public static void AddPlanet(int planetId)
    {
        if(planetMapping.TryGetValue(planetId, out var mapping))
        {
            if (mapping.getter())
            {
                AddItem(mapping.pieceId, 20);
            }
            else
            {
                mapping.setter(true);
            }
        }
    }

    public static void AddReward(int rewardType, int targetId, int amount)
    {
        if(rewardType == (int)RewardType.Planet)
        {
            AddPlanet(targetId);
        }
        else
        {
            AddItem(targetId, amount);
        }
    }
}
