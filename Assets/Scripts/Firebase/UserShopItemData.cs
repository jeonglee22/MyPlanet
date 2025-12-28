using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class UserShopItemData
{
    public List<bool> dailyShop;
    public bool packageShop;

    public List<BuyItemData> buyedItems;

    public bool isUsedReroll;

    public UserShopItemData()
    {
        dailyShop = new List<bool>();
        packageShop = false;
        for (int i = 0; i < 6; i++)
        {
            dailyShop.Add(false);
        }
        buyedItems = new List<BuyItemData>();
        for (int i = 0; i < 6; i++)
        {
            buyedItems.Add(new BuyItemData());
        }
        isUsedReroll = false;
    }

    public UserShopItemData(List<bool> dailyShop, bool packageShop, List<BuyItemData> buyedItems, bool isUsedReroll = false)
    {
        this.dailyShop = dailyShop;
        this.packageShop = packageShop;
        this.buyedItems = buyedItems;
        this.isUsedReroll = isUsedReroll;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserShopItemData FromJson(string json)
    {
        return JsonUtility.FromJson<UserShopItemData>(json);
    }
}

[Serializable]
public class BuyItemData
{
    public int itemId;
    public int count;

    public BuyItemData()
    {
        itemId = 0;
        count = 0;
    }

    public BuyItemData(int itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
    }
}
