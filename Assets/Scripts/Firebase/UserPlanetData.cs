using System;
using UnityEngine;

[Serializable]
public class UserPlanetData
{
    public string nickName;

    public int planetId;
    public int planetUpgrade;
    public int planetLevel;
    public int planetCollectionStat;
    public int towerId;

    public UserPlanetData()
    {
        nickName = string.Empty;
        planetId = 0;
        planetUpgrade = 0;
        planetLevel = 0;
        planetCollectionStat = 0;
        towerId = 0;
    }

    public UserPlanetData(string nickName, int planetId = 0, int planetUpgrade = 0,
        int planetLevel = 1, int planetCollectionStat = 0, int towerId = 0)
    {
        this.nickName = nickName;
        if (planetId == 0)
            planetId = Variables.planetId;
        this.planetId = planetId;
        this.planetUpgrade = planetUpgrade;
        this.planetLevel = planetLevel;
        this.planetCollectionStat = planetCollectionStat;
        this.towerId = towerId;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserPlanetData FromJson(string json)
    {
        return JsonUtility.FromJson<UserPlanetData>(json);
    }
}