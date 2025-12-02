using System;
using UnityEngine;

[Serializable]
public class UserPlanetData
{
    public string nickName;
    public int attackPower;


    public UserPlanetData()
    {
        nickName = string.Empty;
        attackPower = 0;
    }

    public UserPlanetData(string nickName, int attackPower = 0)
    {
        this.nickName = nickName;
        this.attackPower = attackPower;
    }

    public int GetAttackPower()
    {
        return attackPower;
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