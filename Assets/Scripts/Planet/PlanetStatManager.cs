using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlanetStatManager : MonoBehaviour
{
    private static PlanetStatManager instance;
    public static PlanetStatManager Instance => instance;

    private PlanetStats currentPlanetStats;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    public PlanetStats CurrentPlanetStats => currentPlanetStats;

    public int HP => CurrentPlanetStats?.HpInt ?? 0;
    public int Defense => CurrentPlanetStats?.DefenseInt ?? 0;
    public int Shield => CurrentPlanetStats?.ShieldInt ?? 0;
    public int ExpRate => CurrentPlanetStats?.ExpRateInt ?? 0;
    public int Drain => CurrentPlanetStats?.DrainInt ?? 0;
    public int HPRegeneration => CurrentPlanetStats?.HpRegenerationInt ?? 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => PlanetManager.Instance != null && PlanetManager.Instance.IsInitialized);

        UpdateCurrentPlanetStats();

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    public void UpdateCurrentPlanetStats()
    {
        if(PlanetManager.Instance == null)
        {
            return;
        }

        int activePlanetId = PlanetManager.Instance.ActivePlanetId;
        if(activePlanetId < 0)
        {
            currentPlanetStats = new PlanetStats();
            return;
        }

        var planetInfo = PlanetManager.Instance.GetPlanetInfo(activePlanetId);
        if(planetInfo == null)
        {
            currentPlanetStats = new PlanetStats();
            return;
        }

        currentPlanetStats = CalculatePlanetStats(activePlanetId, planetInfo.level, planetInfo.starLevel);
    }

    public PlanetStats GetPlanetStatsPreview(int planetId, int level, int starLevel)
    {
        return CalculatePlanetStats(planetId, level, starLevel);
    }

    public PlanetStats CalculatePlanetStats(int planetId, int level, int starLevel)
    {
        var baseStats = GetBasePlanetStats(planetId);
        var levelUpStats = CalculateLevelUpPlanetStats(planetId, level, starLevel);
        var starUpgradeStats = CalculateStarUpgradeStats(planetId, starLevel);

        return baseStats + levelUpStats + starUpgradeStats;
    }

    public PlanetStats CalculateLevelUpPlanetStats(int planetId, int level, int starLevel)
    {
        float levelHp = 0;
        float levelDefense = 0;

        if(level > 0)
        {
            var lvUpgradeDatas = DataTableManager.PlanetLvUpgradeTable.GetStackLevelData(planetId, level);
            if(lvUpgradeDatas != null)
            {
                foreach(var data in lvUpgradeDatas)
                {
                    levelHp += data.AddHp;
                    levelDefense += data.AddArmor;
                }
            }
        }

        return new PlanetStats
        (
            Mathf.Round(levelHp),
            Mathf.Round(levelDefense),
            0,
            0,
            0,
            0
        );
    }

    public PlanetStats CalculateStarUpgradeStats(int planetId, int starLevel)
    {
        var planetData = DataTableManager.PlanetTable.Get(planetId);
        if(planetData == null)
        {
            return new PlanetStats();
        }

        int baseHp = planetData.PlanetHp;
        int baseDefense = planetData.PlanetArmor;

        float starHp = 0f;
        float starDefense = 0f;
        float starExpRate = 0f;
        float starHpRegeneration = 0f;
        float starShield = 0f;
        float starDrain = 0f;

        float hpPercentBonus = 0f;
        float defensePercentBonus = 0f;

        if(starLevel > 0)
        {
            var starUpgradeDatas = DataTableManager.PlanetStarUpgradeTable.GetStackStarData(planetId, starLevel);
            if(starUpgradeDatas != null)
            {
                foreach(var data in starUpgradeDatas)
                {
                    switch((PlanetAbilityType)data.PlanetAbilityType)
                    {
                        case PlanetAbilityType.Health:
                            starHp += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.Defense:
                            starDefense += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.ExperienceRate:
                            starExpRate += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.RegenerationHP:
                            starHpRegeneration += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.Shield:
                            starShield += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.Drain:
                            starDrain += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.HealthPercentage:
                            hpPercentBonus += data.PlanetAbilityValue;
                            break;
                        case PlanetAbilityType.DefensePercentage:
                            defensePercentBonus += data.PlanetAbilityValue;
                            break;
                    }
                }
            }
        }

        float tempHp = starHp;
        if(hpPercentBonus > 0f)
        {
            tempHp += baseHp * hpPercentBonus / 100f;
        }

        float tempDefense = starDefense;
        if(defensePercentBonus > 0f)
        {
            tempDefense += baseDefense * defensePercentBonus / 100f;
        }

        return new PlanetStats
        (
            Mathf.Round(tempHp),
            Mathf.Round(tempDefense),
            Mathf.Round(starShield),
            Mathf.Round(starExpRate),
            Mathf.Round(starDrain),
            Mathf.Round(starHpRegeneration)
        );
    }

    public PlanetStats GetBasePlanetStats(int planetId)
    {
        var planetData = DataTableManager.PlanetTable.Get(planetId);
        if(planetData == null)
        {
            return new PlanetStats();
        }

        return new PlanetStats(
            planetData.PlanetHp,
            planetData.PlanetArmor,
            planetData.PlanetShield,
            planetData.ExpScale,
            planetData.Drain,
            planetData.RecoveryHp
        );
    }
}
