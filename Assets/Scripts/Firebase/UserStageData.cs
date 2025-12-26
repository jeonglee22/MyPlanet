using UnityEngine;
using System;

[Serializable]
public class UserStageData
{
    public int HighestClearedStage;

    public UserStageData()
    {
        HighestClearedStage = 1;
    }

    public UserStageData(int highestClearedStage)
    {
        this.HighestClearedStage = highestClearedStage;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static UserStageData FromJson(string json)
    {
        return JsonUtility.FromJson<UserStageData>(json);
    }
}
