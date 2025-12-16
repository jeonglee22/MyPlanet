using System;
using UnityEngine;

[Serializable]
public class UserCurrencyData
{
    public int gold = 10000000;
    public int freeDia = 200000;
    public int chargedDia = 300000;

    public UserCurrencyData()
    {
        gold = 10000000;
        freeDia = 200000;
        chargedDia = 300000;
    }

    public UserCurrencyData(int gold, int freeDia, int chargedDia)
    {
        this.gold = gold;
        this.freeDia = freeDia;
        this.chargedDia = chargedDia;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserCurrencyData FromJson(string json)
    {
        return JsonUtility.FromJson<UserCurrencyData>(json);
    }
}
