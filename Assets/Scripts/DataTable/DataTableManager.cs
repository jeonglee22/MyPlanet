using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class DataTableManager
{
    private static readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    private static bool isInitialized = false;
    public static bool IsInitialized => isInitialized;

    public static async UniTask InitializeAsync()
    {
        if (isInitialized)
        {
            return;
        }

        await InitializeTableAsync();
        isInitialized = true;
    }

    private static async UniTask InitializeTableAsync()
    {
        var tasks = new List<UniTask>
        {
            LoadTableAsync<StringTable>(DataTableIds.String),
            LoadTableAsync<EnemyTable>(DataTableIds.Enemy)
        };

        await UniTask.WhenAll(tasks);
    }
    
    private static async UniTask LoadTableAsync<T>(string id) where T : DataTable, new()
    {
        var table = new T();
        await table.LoadAsync(id);
        tables.Add(id, table);
    }

    public static StringTable StringTable
    {
        get
        {
            return Get<StringTable>(DataTableIds.String);
        }
    }

    public static EnemyTable EnemyTable
    {
        get
        {
            return Get<EnemyTable>(DataTableIds.Enemy);
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
