using System;
using UnityEngine;

[Serializable]
public class UserCurrencyData
{
    public int gold = 100000;
    public int freeDia = 2000;
    public int chargedDia = 3000;

    public UserCurrencyData()
    {
        gold = 100000;
        freeDia = 2000;
        chargedDia = 3000;
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
