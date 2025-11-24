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
            LoadTableAsync<EnemyTable>(DataTableIds.Enemy),
            LoadTableAsync<CombineTable>(DataTableIds.Combine),
            LoadTableAsync<WaveTable>(DataTableIds.Wave),
            LoadTableAsync<ProjectileTable>(DataTableIds.Projectile),
            LoadTableAsync<RandomAbilityTable>(DataTableIds.RandomAbility),
            LoadTableAsync<RandomAbilityGroupTable>(DataTableIds.RandomAbilityGroup),
        };

        await UniTask.WhenAll(tasks);
    }
    
    private static async UniTask LoadTableAsync<T>(string id) where T : DataTable, new()
    {
        var table = new T();
        await table.LoadAsync(id);
        tables.Add(id, table);
    }

    public static EnemyTable EnemyTable
    {
        get
        {
            return Get<EnemyTable>(DataTableIds.Enemy);
        }
    }

    public static CombineTable CombineTable
    {
        get
        {
            return Get<CombineTable>(DataTableIds.Combine);
        }
    }

    public static WaveTable WaveTable
    {
        get
        {
            return Get<WaveTable>(DataTableIds.Wave);
        }
    }

    public static ProjectileTable ProjectileTable
    {
        get
        {
            return Get<ProjectileTable>(DataTableIds.Projectile);
        }
    }

    public static RandomAbilityTable RandomAbilityTable
    {
        get
        {
            return Get<RandomAbilityTable>(DataTableIds.RandomAbility);
        }
    }

    public static RandomAbilityGroupTable RandomAbilityGroupTable
    {
        get
        {
            return Get<RandomAbilityGroupTable>(DataTableIds.RandomAbilityGroup);
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
