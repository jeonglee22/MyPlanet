using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserTowerUpgradeData
{
    public List<int> towerIds;
    public List<int> upgradeLevels;

    public UserTowerUpgradeData()
    {
        towerIds = new List<int>();
        upgradeLevels = new List<int>();
        for(int i = 0; i < 6; i++)
        {
            var towerId = i switch
            {
                0 => (int)AttackTowerId.basicGun, // Gun Tower
                1 => (int)AttackTowerId.Missile, // Missile Tower
                2 => (int)AttackTowerId.Gattling, // Gatling Tower
                3 => (int)AttackTowerId.ShootGun, // Shoot Tower
                4 => (int)AttackTowerId.Sniper, // Sniper Tower
                5 => (int)AttackTowerId.Lazer, // Laser Tower
                _ => 0
            };
            towerIds.Add(towerId);
            upgradeLevels.Add(0);
        }
    }

    public UserTowerUpgradeData(List<int> towerIds, List<int> upgradeLevels)
    {
        this.towerIds = towerIds;
        this.upgradeLevels = upgradeLevels;

    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserTowerUpgradeData FromJson(string json)
    {
        return JsonUtility.FromJson<UserTowerUpgradeData>(json);
    }
}
