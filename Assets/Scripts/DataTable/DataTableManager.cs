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
            LoadTableAsync<AttackTowerTable>(DataTableIds.AttackTower),
            LoadTableAsync<BuffTowerTable>(DataTableIds.BuffTower),
            LoadTableAsync<SpecialEffectCombinationTable>(DataTableIds.SpecialEffectCombination),
            LoadTableAsync<SpecialEffectTable>(DataTableIds.SpecialEffect),
            LoadTableAsync<PatternTable>(DataTableIds.Pattern),
            LoadTableAsync<MinionSpawnTable>(DataTableIds.MinionSpawn),
            LoadTableAsync<PlanetLevelUpTable>(DataTableIds.PlanetLevelUp),
            LoadTableAsync<TowerReinforceUpgradeTable>(DataTableIds.TowerReinforceUpgrade),
            LoadTableAsync<BuffTowerReinforceUpgradeTable>(DataTableIds.BuffTowerReinforceUpgrade),
            LoadTableAsync<AsyncPlanetTable>(DataTableIds.AsyncPlanet),
            LoadTableAsync<TowerExplainTable>(DataTableIds.TowerExplain),
            LoadTableAsync<CurrencyTable>(DataTableIds.Currency),
            LoadTableAsync<DrawTable>(DataTableIds.Draw),
            LoadTableAsync<RewardTable>(DataTableIds.Reward),
            LoadTableAsync<ItemTable>(DataTableIds.Item),
            LoadTableAsync<PlanetTable>(DataTableIds.planet),
            LoadTableAsync<RandomAbilityTextTable>(DataTableIds.RandomAbilityText),
            LoadTableAsync<RandomAbilityReinforceUpgradeTable>(DataTableIds.RandomAbilityReinforceUpgrade),
            LoadTableAsync<TowerUpgradeTable>(DataTableIds.TowerUpgrade),
            LoadTableAsync<TowerUpgradeAbilityUnlockTable>(DataTableIds.TowerUpgradeAbilityUnlock),
            LoadTableAsync<SkillTable>(DataTableIds.Skill),
            LoadTableAsync<StageTable>(DataTableIds.Stage),
            LoadTableAsync<PlanetLvUpgradeTable>(DataTableIds.PlanetLvUpgrade),
            LoadTableAsync<PlanetStarUpgradeTable>(DataTableIds.PlanetStarUpgrade),
            LoadTableAsync<LobbyStringTable>(DataTableIds.LobbyString),
            LoadTableAsync<SpecialEffectTextTable>(DataTableIds.SpecialEffectText),
            LoadTableAsync<PlanetTextTable>(DataTableIds.PlanetText),
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
    public static AttackTowerTable AttackTowerTable
    {
        get { return Get<AttackTowerTable>(DataTableIds.AttackTower); }
    }
    public static BuffTowerTable BuffTowerTable
    {
        get
        {
            return Get<BuffTowerTable>(DataTableIds.BuffTower);
        }
    }

    public static SpecialEffectCombinationTable SpecialEffectCombinationTable
    {
        get
        {
            return Get<SpecialEffectCombinationTable>(DataTableIds.SpecialEffectCombination);
        }
    }

    public static SpecialEffectTable SpecialEffectTable
    {
        get
        {
            return Get<SpecialEffectTable>(DataTableIds.SpecialEffect);
        }
    }

    public static PatternTable PatternTable
    {
        get
        {
            return Get<PatternTable>(DataTableIds.Pattern);
        }
    }

    public static MinionSpawnTable MinionSpawnTable
    {
        get
        {
            return Get<MinionSpawnTable>(DataTableIds.MinionSpawn);
        }
    }

    public static PlanetLevelUpTable PlanetLevelUpTable
    {
        get
        {
            return Get<PlanetLevelUpTable>(DataTableIds.PlanetLevelUp);
        }
    }

    public static TowerReinforceUpgradeTable TowerReinforceUpgradeTable
    {
        get { return Get<TowerReinforceUpgradeTable>(DataTableIds.TowerReinforceUpgrade); }
    }

    public static BuffTowerReinforceUpgradeTable BuffTowerReinforceUpgradeTable
    {
        get { return Get<BuffTowerReinforceUpgradeTable>(DataTableIds.BuffTowerReinforceUpgrade); }
    }

    public static AsyncPlanetTable AsyncPlanetTable
    {
        get { return Get<AsyncPlanetTable>(DataTableIds.AsyncPlanet); }
    }

    public static TowerExplainTable TowerExplainTable
    {
        get { return Get<TowerExplainTable>(DataTableIds.TowerExplain); }
    }

    public static CurrencyTable CurrencyTable
    {
        get 
        { 
            return Get<CurrencyTable>(DataTableIds.Currency); 
        }
    }

    public static DrawTable DrawTable
    {
        get 
        { 
            return Get<DrawTable>(DataTableIds.Draw); 
        }
    }

    public static RewardTable RewardTable
    {
        get 
        { 
            return Get<RewardTable>(DataTableIds.Reward); 
        }
    }

    public static ItemTable ItemTable
    {
        get
        {
            return Get<ItemTable>(DataTableIds.Item);
        }
    }

    public static PlanetTable PlanetTable
    {
        get
        {
            return Get<PlanetTable>(DataTableIds.planet);
        }
    }

    public static RandomAbilityTextTable RandomAbilityTextTable
    {
        get
        {
            return Get<RandomAbilityTextTable>(DataTableIds.RandomAbilityText);
        }
    }

    public static TowerUpgradeTable TowerUpgradeTable
    {
        get
        {
            return Get<TowerUpgradeTable>(DataTableIds.TowerUpgrade);
        }
    }

    public static TowerUpgradeAbilityUnlockTable TowerUpgradeAbilityUnlockTable
    {
        get
        {
            return Get<TowerUpgradeAbilityUnlockTable>(DataTableIds.TowerUpgradeAbilityUnlock);
        }
    }
    
    public static SkillTable SkillTable
    {
        get
        {
            return Get<SkillTable>(DataTableIds.Skill);
        }
    }

    public static StageTable StageTable
    {
        get
        {
            return Get<StageTable>(DataTableIds.Stage);
        }
    }

    public static RandomAbilityReinforceUpgradeTable RandomAbilityReinforceUpgradeTable 
    {
        get 
        {
            return Get<RandomAbilityReinforceUpgradeTable>(DataTableIds.RandomAbilityReinforceUpgrade);
        }
    }

    public static PlanetLvUpgradeTable PlanetLvUpgradeTable
    {
        get
        {
            return Get<PlanetLvUpgradeTable>(DataTableIds.PlanetLvUpgrade);
        }
    }

    public static PlanetStarUpgradeTable PlanetStarUpgradeTable
    {
        get
        {
            return Get<PlanetStarUpgradeTable>(DataTableIds.PlanetStarUpgrade);
        }
    }

    public static LobbyStringTable LobbyStringTable
    {
        get
        {
            return Get<LobbyStringTable>(DataTableIds.LobbyString);
        }
    }

    public static SpecialEffectTextTable SpecialEffectTextTable
    {
        get
        {
            return Get<SpecialEffectTextTable>(DataTableIds.SpecialEffectText);
        }
    }

    public static PlanetTextTable PlanetTextTable
    {
        get
        {
            return Get<PlanetTextTable>(DataTableIds.PlanetText);
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
