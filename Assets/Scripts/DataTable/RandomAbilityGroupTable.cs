using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RandomAbilityGroupData
{
    public int RandomAbilityGroup_ID { get; set; }
    public string RandomAbilityGroup {get; set; }
    public int TowerType { get; set; }

    public List<int> RandomAbilityGroupList { get; set; }

    public RandomAbilityGroupData()
    {
        RandomAbilityGroupList = new List<int>();
    }

    public void SplitRandomAbilityGroup()
    {
        RandomAbilityGroupList.Clear();
        if (string.IsNullOrWhiteSpace(RandomAbilityGroup)) return;

        var cleaned = RandomAbilityGroup.Trim();
        cleaned = cleaned.Trim('\"');
        cleaned = cleaned.TrimStart('[').TrimEnd(']');
        var items = cleaned.Split(',');
        foreach (var raw in items)
        {
            var s = raw.Trim();
            if (int.TryParse(s, out int abilityID))
                RandomAbilityGroupList.Add(abilityID);
            else
                Debug.LogWarning($"RandomAbilityGroup 파싱 실패: '{raw}' (원문: '{RandomAbilityGroup}')");
        }
    }
}
public class RandomAbilityGroupTable : DataTable
{
    private readonly Dictionary<int, RandomAbilityGroupData> dictionary = new Dictionary<int, RandomAbilityGroupData>();

    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<RandomAbilityGroupData>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.RandomAbilityGroup_ID, item))
            {
                Debug.LogError($"키 중복: {item.RandomAbilityGroup_ID}");
            }
            item.SplitRandomAbilityGroup();
        }
    }

    public RandomAbilityGroupData Get(int key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return null;
        }

        return dictionary[key];
    }

    public int GetRandomAbilityInGroup(int key)
    {
        var data = Get(key);
        if (data == null || data.RandomAbilityGroupList.Count == 0)
            return -1;

        var index = Random.Range(0, data.RandomAbilityGroupList.Count);
        var picked = data.RandomAbilityGroupList[index];

        Debug.Log($"[RandomAbilityPick] group={key} index={index}/{data.RandomAbilityGroupList.Count} picked={picked}");

        return picked;
    }
}
