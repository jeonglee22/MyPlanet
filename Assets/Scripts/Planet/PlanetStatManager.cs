using Cysharp.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlanetStatManager : MonoBehaviour
{
    private static PlanetStatManager instance;
    public static PlanetStatManager Instance => instance;

    private PlanetStats currentPlanetStats;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    public PlanetStats CurrentPlanetStats => currentPlanetStats;

    public int HP => CurrentPlanetStats?.hp ?? 0;
    public int Defense => CurrentPlanetStats?.defense ?? 0;
    public int Shield => CurrentPlanetStats?.shield ?? 0;
    public float ExpRate => CurrentPlanetStats?.expRate ?? 0f;
    public int Drain => CurrentPlanetStats?.drain ?? 0;
    public float HPRegeneration => CurrentPlanetStats?.hpRegeneration ?? 0f;

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
            Mathf.RoundToInt(levelHp),
            Mathf.RoundToInt(levelDefense),
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
            Mathf.RoundToInt(tempHp),
            Mathf.RoundToInt(tempDefense),
            Mathf.RoundToInt(starShield),
            Mathf.RoundToInt(starExpRate),
            Mathf.RoundToInt(starDrain),
            Mathf.RoundToInt(starHpRegeneration)
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
            Mathf.RoundToInt(planetData.PlanetHp),
            Mathf.RoundToInt(planetData.PlanetArmor),
            Mathf.RoundToInt(planetData.PlanetShield),
            Mathf.RoundToInt(planetData.ExpScale),
            Mathf.RoundToInt(planetData.Drain),
            Mathf.RoundToInt(planetData.RecoveryHp)
        );
    }
}
