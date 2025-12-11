using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserTowerData
{
    public int towerId;
    public int towerLevelId;
    public int towerInitDataId;
    // public ProjectileData buffedProjectileData;

    public float AttackType;
    public float TargetNum;
    public float HitType;
    public float CollisionSize;
    public float Attack;
    public float FixedPenetration;
    public float RatePenetration;
    public float ProjectileSpeed;
    public float ProjectileAddSpeed ;
    public float RemainTime;

    public List<int> abilities;

    public UserTowerData()
    {
        towerId = 0;
        towerLevelId = 0;
        towerInitDataId = 0;
        // buffedProjectileData = null;
        AttackType = 0;
        TargetNum = 0;
        HitType = 0;
        CollisionSize = 0;
        Attack = 0;
        FixedPenetration = 0;
        RatePenetration = 0;
        ProjectileSpeed = 0;
        ProjectileAddSpeed = 0;
        RemainTime = 0;
        abilities = new List<int>();
    }

    public UserTowerData(int towerId, int towerLevelId = -1,
        int initTowerData = 0, ProjectileData buffedProjectileData = null, List<int> abilities = null)
    {
        this.towerId = towerId;
        this.towerLevelId = towerLevelId;
        this.towerInitDataId = initTowerData;
        
        AttackType = buffedProjectileData?.AttackType ?? 0;
        TargetNum = buffedProjectileData?.TargetNum ?? 0;
        HitType = buffedProjectileData?.HitType ?? 0;
        CollisionSize = buffedProjectileData?.CollisionSize ?? 0;
        Attack = buffedProjectileData?.Attack ?? 0;
        FixedPenetration = buffedProjectileData?.FixedPenetration ?? 0;
        RatePenetration = buffedProjectileData?.RatePenetration ?? 0;
        ProjectileSpeed = buffedProjectileData?.ProjectileSpeed ?? 0;
        ProjectileAddSpeed = buffedProjectileData?.ProjectileAddSpeed ?? 0;
        RemainTime = buffedProjectileData?.RemainTime ?? 0;

        // this.buffedProjectileData = buffedProjectileData;
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
