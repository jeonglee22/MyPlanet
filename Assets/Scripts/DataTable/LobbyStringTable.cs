using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LobbyStringData
{
    public string Key { get; set; }
    public string Text { get; set; }
}

public class LobbyStringTable : DataTable
{
    private readonly Dictionary<string, LobbyStringData> dictionary = new Dictionary<string, LobbyStringData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<LobbyStringData>(textAsset.text);

        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Key, item))
            {
                Debug.LogError($"키 중복: {item.Key}");
            }
        }
    }

    public string GetString(string key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key].Text;
    }
}

// public enum LobbyStringKey
// {
//     UI_Start_TouchtoStart,
//     UI_LogIn_Title,
//     UI_LogIn_Google,
//     UI_LogIn_Failed,
//     UI_Name_Enter,
//     UI_Name_Rule,
//     UI_Name_Error1,
//     UI_Name_Error2,
//     UI_Name_EnterButton,
//     UI_LogIn_NameConfirm,
//     UI_Loading,
//     UI_ButtonText_PlanetUpgrade,
//     UI_ButtonText_TowerUpgrade,
//     UI_ButtonText_Collection,
//     UI_ButtonText_Store,
//     UI_ButtonText_Play,
//     UI_Setting_Title,
//     UI_Setting_BGM,
//     UI_Setting_SFX,
//     UI_Setting_BGM_MinValue,
//     UI_Setting_BGM_MaxValue,
//     UI_Setting_SFX_MinValue,
//     UI_Setting_SFX_MaxValue,
//     StageName50001,
//     StageName50002,
//     StageName50003,
//     StageName50004,
//     StageName50005,
//     StageName50006,
//     StageName50007,
//     BossName50001,
//     BossName50002,
//     BossName50003,
//     BossName50004,
//     BossName50005,
//     BossName50006,
//     BossName50007,
//     LockedStageName50001,
//     LockedStageName50002,
//     LockedStageName50003,
//     LockedStageName50004,
//     LockedStageName50005,
//     LockedStageName50006,
//     LockedStageName50007,
//     LockedBossName
// }
