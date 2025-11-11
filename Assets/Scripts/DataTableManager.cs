using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class DataTableManager
{
    private static readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    private static bool isInitialized = false;
    private static UniTask initializationTask;

    private static async UniTask InitAsync()
    {
        if (isInitialized)
        {
            return;
        }
    }

    public static StringTable StringTable
    {
        get
        {
            return Get<StringTable>(DataTableIds.String);
        }
    }
    
    public static T Get<T>(string id) where T : DataTable
    {
        if (!tables.ContainsKey(id))
        {
            Debug.LogError($"데이터테이블 없음: {id}");
            return null;
        }

        return tables[id] as T;
    }
}
