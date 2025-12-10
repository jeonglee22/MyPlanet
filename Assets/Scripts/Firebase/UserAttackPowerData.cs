using System;
using UnityEngine;

[Serializable]
public class UserAttackPowerData
{
    public int attackPower;
    public long timeStamp;

    public UserAttackPowerData()
    {
        attackPower = 0;
        timeStamp = 0;
    }

    public UserAttackPowerData(int attackPower, long timeStamp)
    {
        this.attackPower = attackPower;
        this.timeStamp = timeStamp;
    }

    public DateTime GetDateTime()
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserAttackPowerData FromJson(string json)
    {
        return JsonUtility.FromJson<UserAttackPowerData>(json);
    }

    public void CalculateAttackPower(UserPlanetData planetData)
    {
        // planetData 기반으로 공격력 계산 로직 구현
        // planetTable 연결 필요

        attackPower = planetData.planetLevel * 100 + planetData.planetUpgrade * 50;
    }
}
