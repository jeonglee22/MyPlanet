using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserTowerData
{
    public int towerId;
    public int towerLevelId;
    public TowerDataSO buffedTowerData;
    public ProjectileData buffedProjectileData;

    public List<int> abilities;

    public UserTowerData()
    {
        towerId = 0;
        towerLevelId = 0;
        buffedTowerData = null;
        buffedProjectileData = null;
        abilities = new List<int>();
    }

    public UserTowerData(int towerId, int towerLevelId,
        TowerDataSO buffedTowerData, ProjectileData buffedProjectileData, List<int> abilities)
    {
        this.towerId = towerId;
        this.towerLevelId = towerLevelId;
        this.buffedTowerData = buffedTowerData;
        this.buffedProjectileData = buffedProjectileData;
        this.abilities = abilities;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserTowerData FromJson(string json)
    {
        return JsonUtility.FromJson<UserTowerData>(json);
    }
}
