using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RandomAbilityGroupData
{
    public int RandomAbilityGroup_ID { get; set; }
    public string RandomAbilityGroup {get; set; }

    public List<int> RandomAbilityGroupList { get; set; }

    public RandomAbilityGroupData()
    {
        RandomAbilityGroupList = new List<int>();
    }
    
    public void SplitRandomAbilityGroup()
    {
        var items = RandomAbilityGroup.Split(',');
        foreach(var item in items)
        {
            if(int.TryParse(item, out int abilityID))
            {
                RandomAbilityGroupList.Add(abilityID);
            }
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

        /* test : data table load check
        foreach(var item in list)
        {
            Debug.Log(item.ToString());
        }
        */
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
        return data.RandomAbilityGroupList[index];
    }
}
