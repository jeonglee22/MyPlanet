using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlanetLevelUpgradeUI : MonoBehaviour
{
    [SerializeField] private Image planetIcon;
    [SerializeField] private TextMeshProUGUI planetNameText;
    [SerializeField] private TextMeshProUGUI attackPowerText;

    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI itemCountText;

    [SerializeField] private Button levelUpButton;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color canUpgradeColor;

    [SerializeField] private PlanetPanelUI planetPanelUI;
    [SerializeField] private PlanetInfoUI planetInfoUI;

    private PlanetData currentPlanetData;
    private UserPlanetInfo currentUserPlanetInfo;
    private PlanetLvUpgradeData planetLvUpgradeData;
    private PlanetStats currentStats;

    private void Start()
    {
        levelUpButton.onClick.AddListener(() => OnLevelUpButtonClicked().Forget());
    }

    public void Initialize(PlanetData planetData, UserPlanetInfo userPlanetInfo)
    {
        currentPlanetData = planetData;
        currentUserPlanetInfo = userPlanetInfo;

        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetImage);

        var planetTextData = DataTableManager.PlanetTextTable.Get(planetData.PlanetText_ID);
        planetNameText.text = planetTextData.PlanetName;

        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        UpdateLevelDisplay();
        UpdateStatsDisplay();
        UpdateFightingPower();
        UpdateItemCount();
        UpdateLevelUpButton();
    }

    private void UpdateLevelDisplay()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;
        currentLevelText.text = $"Lv. {currentUserPlanetInfo.level} / Lv. {maxLevel}";

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            nextLevelText.text = "MAX";
        }
        else
        {
            nextLevelText.text = $"Lv. {currentUserPlanetInfo.level + 1}";
        }
    }

    private void UpdateStatsDisplay()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        var currentStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
            currentPlanetData.Planet_ID, 
            currentUserPlanetInfo.level, 
            currentUserPlanetInfo.starLevel);

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            healthText.text = $"{Mathf.RoundToInt(currentStats.hp)}";
            defenseText.text = $"{Mathf.RoundToInt(currentStats.defense)}";
        }
        else
        {
            var nextStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
                currentPlanetData.Planet_ID, 
                currentUserPlanetInfo.level + 1, 
                currentUserPlanetInfo.starLevel);

            int hpDiff = Mathf.RoundToInt(nextStats.hp - currentStats.hp);
            int defenseDiff = Mathf.RoundToInt(nextStats.defense - currentStats.defense);

            healthText.text = $"{Mathf.RoundToInt(currentStats.hp)} → {Mathf.RoundToInt(nextStats.hp)} " + $"<color=green>(+{hpDiff})</color>";
            defenseText.text = $"{Mathf.RoundToInt(currentStats.defense)} → {Mathf.RoundToInt(nextStats.defense)} " + $"<color=green>(+{defenseDiff})</color>";
        }
    }

    private void UpdateFightingPower()
    {
        var currentStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
            currentPlanetData.Planet_ID, 
            currentUserPlanetInfo.level, 
            currentUserPlanetInfo.starLevel);

        var cal = (currentStats.hp * (100 + currentStats.defense) / 100f) + 
                  currentStats.shield + 
                  (currentStats.hpRegeneration * 420) + 
                  (currentStats.drain * 650);

        attackPowerText.text = $"{Mathf.RoundToInt(cal)}";
    }

    private void UpdateItemCount()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            itemCountText.text = "MAX";
            return;
        }

        int nextLevel = currentUserPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(
            currentPlanetData.Planet_ID, nextLevel);

        if(planetLvUpgradeData == null)
        {
            itemCountText.text = "데이터 없음";
            return;
        }

        int currentItem = UserData.PlanetEnhanceItem;
        int requiredItem = planetLvUpgradeData.UpgradeResource;

        if(currentItem >= requiredItem)
        {
            itemCountText.text = $"{currentItem} / {requiredItem}";
        }
        else
        {
            itemCountText.text = $"<color=red>{currentItem}</color> / {requiredItem}";
        }
    }

    private void UpdateLevelUpButton()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            levelUpButton.interactable = false;
            levelUpButton.GetComponent<Image>().color = defaultColor;
            return;
        }

        int nextLevel = currentUserPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(
            currentPlanetData.Planet_ID, nextLevel);

        if(planetLvUpgradeData == null)
        {
            levelUpButton.interactable = false;
            levelUpButton.GetComponent<Image>().color = defaultColor;
            return;
        }

        int currentItem = UserData.PlanetEnhanceItem;
        int requiredItem = planetLvUpgradeData.UpgradeResource;

        if(currentItem >= requiredItem)
        {
            levelUpButton.interactable = true;
            levelUpButton.GetComponent<Image>().color = canUpgradeColor;
        }
        else
        {
            levelUpButton.interactable = false;
            levelUpButton.GetComponent<Image>().color = defaultColor;
        }
    }

    private async UniTaskVoid OnLevelUpButtonClicked()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            Debug.Log("최대 레벨 도달");
            return;
        }

        int nextLevel = currentUserPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(
            currentPlanetData.Planet_ID, nextLevel);

        if(planetLvUpgradeData == null)
        {
            Debug.LogError("레벨 업그레이드 데이터 없음");
            return;
        }

        int currentItem = UserData.PlanetEnhanceItem;
        int requiredItem = planetLvUpgradeData.UpgradeResource;

        if(currentItem < requiredItem)
        {
            Debug.Log("골드 부족");
            return;
        }

        UserData.PlanetEnhanceItem -= requiredItem;
        await CurrencyManager.Instance.SaveCurrencyAsync();

        PlanetManager.Instance.LevelUpPlanet(currentPlanetData.Planet_ID);
        await PlanetManager.Instance.SavePlanetsAsync();

        if(currentPlanetData.Planet_ID == PlanetManager.Instance.ActivePlanetId)
        {
            PlanetStatManager.Instance?.UpdateCurrentPlanetStats();
        }

        UpdateAllUI();

        planetPanelUI.RefreshPlanetPanelUI();
        planetInfoUI.Initialize(currentPlanetData, currentUserPlanetInfo);
    }

    public void RefreshUI()
    {
        if(currentPlanetData != null && currentUserPlanetInfo != null)
        {
            Initialize(currentPlanetData, currentUserPlanetInfo);
        }
    }
}
