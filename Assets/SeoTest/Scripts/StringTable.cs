using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StringTable : DataTable
{
    public static readonly string Unknown = "키없음";
    public class Data
    {
        public string Id { get; set; }
        public string String { get; set; }
    }

    //readonly를 붙일 수 있는곳을 모두 붙여라 -> 가비지 컬렉션이 이거에 작동하지 않게된다. / readonly라고해서 
    private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

    public override void Load(string fileame)
    {
        dictionary.Clear(); //비어있는 상태로 만들고 새로 채우는 것이다.

        var path = string.Format(FormatPath, fileame); //파일 경로
        var textAsset = Resources.Load<TextAsset>(path);

        var list = LoadCSV<Data>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Id, item.String))
            {
                Debug.LogError($"키 중복: {item.Id}");
            }
        }
    }
    
    public override async UniTask LoadAsync(string filename)
    {
        dictionary.Clear();

        var path = string.Format(FormatPath, filename);
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(path).ToUniTask();

        var list = await LoadCSVAsync<Data>(textAsset.text);
        foreach (var item in list)
        {
            if (!dictionary.TryAdd(item.Id, item.String))
            {
                Debug.LogError($"키 중복: {item.Id}");
            }
        }
    }

    public string Get(string key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return Unknown;
        }

        return dictionary[key];
    }
}