using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerReinforceManager : MonoBehaviour
{
    private static TowerReinforceManager _instance;
    public static TowerReinforceManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("TowerReinforceManager");
            _instance = go.AddComponent<TowerReinforceManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    [Header("요격 타워 공격력 배율 계수")]
    [SerializeField] private float attackReinforceScale = 1f;

    [Header("증폭 타워 강화 배율(전체) 계수")]
    [SerializeField] private float buffReinforceScale = 1f;

    private Dictionary<int, List<TowerReinforceUpgradeRow>> attackGroups =
        new Dictionary<int, List<TowerReinforceUpgradeRow>>();

    private Dictionary<int, List<BuffTowerReinforceUpgradeRow>> buffGroups =
        new Dictionary<int, List<BuffTowerReinforceUpgradeRow>>();

    private bool initialized = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //Init
    private void EnsureInitialized()
    {
        if (initialized) return;
        if (!DataTableManager.IsInitialized) return;
        initialized = true;
    }

    //If You Need Calculator ----------------------------------------
    public float GetAttackAddValue(int groupId, int currentLevel)
    {
        EnsureInitialized();
        if (!initialized) return 0f;
        if (currentLevel <= 0) return 0f;
        if (!attackGroups.TryGetValue(groupId, out var rows)) return 0f;

        int clampedLevel = Mathf.Max(0, currentLevel);

        float sum = 0f;
        foreach (var row in rows)
        {
            if (row.ReinforceUpgradeLevel <= clampedLevel)
            {
                sum += row.AddValue;
            }
        }

        return sum * attackReinforceScale;
    }
    public static float GetAttackAddValueStatic(int groupId, int currentLevel)
    {
        if (Instance == null) return 0f;
        return Instance.GetAttackAddValue(groupId, currentLevel);
    }
    //---------------------------------------------------------------

    //Current Calculator
    //Reinforce Attack Tower
    public float GetAttackAddValueByIds(int[] reinforceIds, int currentLevel)
    {
        EnsureInitialized();
        if (!initialized) return 0f;
        if (reinforceIds == null || reinforceIds.Length == 0) return 0f;
        if (currentLevel <= 0) return 0f;

        int maxLevel = Mathf.Min(currentLevel, reinforceIds.Length);
        var table = DataTableManager.TowerReinforceUpgradeTable;
        if (table == null) return 0f;

        float sum = 0f;

        for (int i = 0; i < maxLevel; i++)
        {
            int id = reinforceIds[i];
            var row = table.GetById(id);
            if (row == null) continue;
            sum += row.AddValue;
        }
        return sum * attackReinforceScale;
    }

    //Reinforce Buff Tower
    public Dictionary<int, float> GetBuffAddValues(int groupId, int currentLevel)
    {
        EnsureInitialized();
        var result = new Dictionary<int, float>();

        if (!initialized) return result;
        if (currentLevel <= 0) return result;
        if (!buffGroups.TryGetValue(groupId, out var rows)) return result;

        int clampedLevel = Mathf.Max(0, currentLevel);

        foreach (var row in rows)
        {
            if (row.ReinforceUpgradeLevel > clampedLevel)
                continue;

            AccumulateEffect(result, row.SpecialEffect1_ID, row.SpecialEffect1AddValue);
            AccumulateEffect(result, row.SpecialEffect2_ID, row.SpecialEffect2AddValue);
            AccumulateEffect(result, row.SpecialEffect3_ID, row.SpecialEffect3AddValue);
        }

        if (!Mathf.Approximately(buffReinforceScale, 1f))
        {
            var keys = new List<int>(result.Keys);
            foreach (var key in keys)
            {
                result[key] *= buffReinforceScale;
            }
        }
        return result;
    }

    public Dictionary<int, float> GetBuffAddValuesByIds(int[] reinforceIds, int currentLevel)
    {
        EnsureInitialized();
        var result = new Dictionary<int, float>();

        if (!initialized) return result;
        if (currentLevel <= 0) return result;
        if (reinforceIds == null || reinforceIds.Length == 0) return result;

        int clampedLevel = Mathf.Max(0, currentLevel);

        var table = DataTableManager.BuffTowerReinforceUpgradeTable;
        if (table == null) return result;

        foreach (var id in reinforceIds)
        {
            var row = table.GetById(id);
            if (row == null) continue;
            if (row.ReinforceUpgradeLevel > clampedLevel) continue;

            AccumulateEffect(result, row.SpecialEffect1_ID, row.SpecialEffect1AddValue);
            AccumulateEffect(result, row.SpecialEffect2_ID, row.SpecialEffect2AddValue);
            AccumulateEffect(result, row.SpecialEffect3_ID, row.SpecialEffect3AddValue);
        }

        if (!Mathf.Approximately(buffReinforceScale, 1f))
        {
            var keys = new List<int>(result.Keys);
            foreach (var key in keys)
            {
                result[key] *= buffReinforceScale;
            }
        }
        return result;
    }

    public static Dictionary<int, float> GetBuffAddValuesStatic(int groupId, int currentLevel)
    {
        if (Instance == null) return new Dictionary<int, float>();
        return Instance.GetBuffAddValues(groupId, currentLevel);
    }

    public static Dictionary<int, float> GetBuffAddValuesByIdsStatic(int[] reinforceIds, int currentLevel)
    {
        if (Instance == null) return new Dictionary<int, float>();
        return Instance.GetBuffAddValuesByIds(reinforceIds, currentLevel);
    }

    private static void AccumulateEffect(Dictionary<int, float> dict, int effectId, float addValue)
    {
        if (effectId == 0) return;
        if (Mathf.Approximately(addValue, 0f)) return;

        if (dict.TryGetValue(effectId, out var current))
        {
            dict[effectId] = current + addValue;
        }
        else
        {
            dict[effectId] = addValue;
        }
    }
}