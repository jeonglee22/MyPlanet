using System;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public static int Gold
    {
        get => CurrencyManager.Instance?.CachedGold ?? 100000;
        set => CurrencyManager.Instance?.SetGold(value);
    }

    public static int FreeDia
    {
        get => CurrencyManager.Instance?.CachedFreeDia ?? 2000;
        set => CurrencyManager.Instance?.SetFreeDia(value);
    }

    public static int ChargedDia
    {
        get => CurrencyManager.Instance?.CachedChargedDia ?? 3000;
        set => CurrencyManager.Instance?.SetChargedDia(value);
    }
    
    public static int TowerEnhanceItem
    {
        get => ItemManager.Instance?.GetItem(710201) ?? 0;
        set => ItemManager.Instance?.SetItem(710201, value);
    }

    public static int PlanetEnhanceItem
    {
        get => ItemManager.Instance?.GetItem(710202) ?? 0;
        set => ItemManager.Instance?.SetItem(710202, value);
    }

    public static int HealthPlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710301) ?? 0;
        set => ItemManager.Instance?.SetItem(710301, value);
    }

    public static int DefensePlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710302) ?? 0;
        set => ItemManager.Instance?.SetItem(710302, value);
    }

    public static int ShieldPlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710303) ?? 0;
        set => ItemManager.Instance?.SetItem(710303, value);
    }

    public static int BloodAbsorbPlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710304) ?? 0;
        set => ItemManager.Instance?.SetItem(710304, value);
    }

    public static int ExpPlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710305) ?? 0;
        set => ItemManager.Instance?.SetItem(710305, value);
    }

    public static int HealthRegenerationPlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710306) ?? 0;
        set => ItemManager.Instance?.SetItem(710306, value);
    }

    public static int CommonPlanetPiece
    {
        get => ItemManager.Instance?.GetItem(710307) ?? 0;
        set => ItemManager.Instance?.SetItem(710307, value);
    }

    public static int TowerDictionaryRate {get; set;} = 0;
    public static int AbilityDictionaryRate {get; set;} = 0;

    public static bool isHealthPlanet = false;
    public static bool isDefensePlanet = false;
    public static bool isShieldPlanet = false;
    public static bool isBloodAbsorbPlanet = false;
    public static bool isExpPlanet = false;
    public static bool isHealthRegenerationPlanet = false;

    public static int CollectionTowerCore
    {
        get => CollectionManager.Instance?.TowerCore ?? 12;
        set => CollectionManager.Instance?.SetTowerCore(value);
    }

    public static int CollectionAbilityCore
    {
        get => CollectionManager.Instance?.AbilityCore ?? 10;
        set => CollectionManager.Instance?.SetAbilityCore(value);
    }
}

public static class UserDataMapper
{
    private static Dictionary<int, (Func<int> getter, Action<int> setter, int maxStack)> itemMapping;
    private static Dictionary<int, (Func<bool> getter, Action<bool> setter, int pieceId)> planetMapping;

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
