using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CollectionData
{
    public int towerCore;
    public int abilityCore;
    public List<WeightEntry> weights = new List<WeightEntry>();

    public CollectionData()
    {
        towerCore = 0;
        abilityCore = 0;
        weights = new List<WeightEntry>();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static CollectionData FromJson(string json)
    {
        return JsonUtility.FromJson<CollectionData>(json);
    }

    public Dictionary<int, float> ToDictionary()
    {
        Dictionary<int, float> weightDict = new Dictionary<int, float>();

        foreach (var weight in weights)
        {
            weightDict[weight.id] = weight.weight;
        }

        return weightDict;
    }

    public static CollectionData FromDictionary(int towerCore, int abilityCore, Dictionary<int, float> weightDict)
    {
        CollectionData collectionData = new CollectionData();
        collectionData.towerCore = towerCore;
        collectionData.abilityCore = abilityCore;

        foreach (var kvp in weightDict)
        {
            collectionData.weights.Add(new WeightEntry(kvp.Key, kvp.Value));
        }

        return collectionData;
    }

}

[Serializable]
public class WeightEntry
{
    public int id;
    public float weight;

    public WeightEntry()
    {
        id = 0;
        weight = 0;
    }

    public WeightEntry(int id, float weight)
    {
        this.id = id;
        this.weight = weight;
    }
}
