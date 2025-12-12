using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemsData
{
    public List<ItemEntry> items = new List<ItemEntry>();

    public ItemsData()
    {
        items = new List<ItemEntry>();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static ItemsData FromJson(string json)
    {
        return JsonUtility.FromJson<ItemsData>(json);
    }

    public Dictionary<int, int> ToDictionary()
    {
        Dictionary<int, int> itemDict = new Dictionary<int, int>();

        foreach (var item in items)
        {
            itemDict[item.itemId] = item.count;
        }

        return itemDict;
    }

    public static ItemsData FromDictionary(Dictionary<int, int> itemDict)
    {
        ItemsData itemsData = new ItemsData();

        foreach (var kvp in itemDict)
        {
            itemsData.items.Add(new ItemEntry(kvp.Key, kvp.Value));
        }

        return itemsData;
    }
}

[Serializable]
public class ItemEntry
{
    public int itemId;
    public int count;

    public ItemEntry()
    {
        itemId = 0;
        count = 0;
    }

    public ItemEntry(int itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
    }
}
