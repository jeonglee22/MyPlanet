using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UserProfile
{
    public string nickName;
    public string email;
    public long createdTime;

    public UserProfile()
    {
        nickName = string.Empty;
        email = string.Empty;
        createdTime = 0;
    }

    public UserProfile(string nickName, string email)
    {
        this.nickName = nickName;
        this.email = email;
        createdTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserProfile FromJson(string json)
    {
        return JsonUtility.FromJson<UserProfile>(json);
    }
}