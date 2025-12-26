using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetStarUpgradePanelUI : MonoBehaviour
{
    [SerializeField] private Image planetIcon;
    [SerializeField] private TextMeshProUGUI planetNameText;
    [SerializeField] private TextMeshProUGUI fightingPowerText;

    [SerializeField] private List<TextMeshProUGUI> upgradeName;
    [SerializeField] private List<TextMeshProUGUI> upgradeValue;

    [SerializeField] private List<GameObject> notUpgradeImage;

    [SerializeField] private List<GameObject> upgradeStar;
    [SerializeField] private Slider pieceSlider;
    [SerializeField] private Sprite changePieceImage;
    [SerializeField] private Sprite defaultPieceImage;
    [SerializeField] private TextMeshProUGUI pieceText;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color canUpgradeColor;

    [SerializeField] private PlanetPanelUI planetPanelUI;
    [SerializeField] private PlanetInfoUI planetInfoUI;

    private List<PlanetStarUpgradeData> upgradeDataList = new List<PlanetStarUpgradeData>();
    private PlanetStarUpgradeData currentPlanetStarUpgradeData;
    private PlanetStarUpgradeData nextPlanetStarUpgradeData;
    private PlanetData currentPlanetData;
    private UserPlanetInfo currentUserPlanetInfo;

    private void Start()
    {
        upgradeButton.onClick.AddListener(() => OnUpgradeButtonClicked().Forget());
        upgradeButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
    }

    public void Initialize(PlanetData planetData, UserPlanetInfo userPlanetInfo)
    {
        var textData = DataTableManager.PlanetTextTable.Get(planetData.PlanetText_ID);
        currentPlanetData = planetData;
        currentUserPlanetInfo = userPlanetInfo;

        upgradeDataList = DataTableManager.PlanetStarUpgradeTable.GetAllDataByPlanetId(planetData.Planet_ID);

        currentPlanetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, userPlanetInfo.starLevel + 1);
        
        int nextStarLevel = userPlanetInfo.starLevel + 1;
        if(nextStarLevel <= PlanetManager.Instance.MaxStarLevel)
        {
            nextPlanetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, userPlanetInfo.starLevel);
        }

        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetImage);
        planetNameText.text = textData.PlanetName;

        UpdateAllUI();

    }

    private void UpdateAllUI()
    {
        UpdateStarDisplay();
        UpdateUpgradeInfo();
        UpdatePieceSlider();
        UpdateFightingPower();
        UpdateUpgradeButton();
    }

    private void UpdateStarDisplay()
    {
        for(int i = 0; i < upgradeStar.Count; i++)
        {
            upgradeStar[i].SetActive(i < currentUserPlanetInfo.starLevel);
        }

        for(int i = 0; i < notUpgradeImage.Count; i++)
        {
            notUpgradeImage[i].SetActive(i + 2 > currentUserPlanetInfo.starLevel);
        }
    }

    private void UpdateUpgradeInfo()
    {
        for(int i = 0; i < upgradeName.Count; i++)
        {
            int displayStarLevel = i + 1;

            if(displayStarLevel <= upgradeDataList.Count)
            {
                var data = upgradeDataList[displayStarLevel];
            
                string abilityName = GetAbilityTypeName((PlanetAbilityType)data.PlanetAbilityType);
                upgradeName[i].text = abilityName;

                upgradeValue[i].text = GetAbilityValueText((PlanetAbilityType)data.PlanetAbilityType, data.PlanetAbilityValue);
            }
        }
    }

    private void UpdatePieceSlider()
    {
        if(currentPlanetStarUpgradeData == null)
        {
            pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;
            pieceSlider.value = pieceSlider.maxValue;
            pieceText.text = "MAX";
            return;
        }

        int pieceId = currentPlanetData.PieceId;

        int currentPieces = ItemManager.Instance.GetItem(pieceId);

        int requiredPieces = currentPlanetStarUpgradeData?.UpgradeResource ?? 0;

        if(currentPieces >= requiredPieces)
        {
            pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;
        }
        else
        {
            pieceSlider.fillRect.GetComponent<Image>().sprite = defaultPieceImage;
        }

        pieceSlider.maxValue = requiredPieces;
        pieceSlider.value = currentPieces;

        pieceText.text = $"{currentPieces} / {requiredPieces}";
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

        fightingPowerText.text = $"{Mathf.RoundToInt(cal)}";
    }

    private string GetAbilityTypeName(PlanetAbilityType abilityType)
    {
        switch (abilityType)
        {
            case PlanetAbilityType.None:
                return "없음";
            case PlanetAbilityType.Health:
                return "체력";
            case PlanetAbilityType.Defense:
                return "방어력";
            case PlanetAbilityType.HealthPercentage:
                return "체력 %";
            case PlanetAbilityType.DefensePercentage:
                return "방어력 %";
            case PlanetAbilityType.Shield:
                return "보호막";
            case PlanetAbilityType.Drain:
                return "흡혈";
            case PlanetAbilityType.RegenerationHP:
                return "체력 재생";
            case PlanetAbilityType.ExperienceRate:
                return "경험치 배율";
            default:
                return "알 수 없음";
        }
    }

    private string GetAbilityValueText(PlanetAbilityType abilityType, float value)
    {
        switch (abilityType)
        {
            case PlanetAbilityType.HealthPercentage:
            case PlanetAbilityType.DefensePercentage:
            case PlanetAbilityType.ExperienceRate:
                return $"+{value}%";
            default:
                return $"+{Mathf.Round(value)}";
        }
    }

    private void UpdateUpgradeButton()
    {
        if(currentPlanetStarUpgradeData == null)
        {
            upgradeButton.interactable = false;
            upgradeButton.GetComponent<Image>().color = defaultColor;
            return;
        }

        int pieceId = currentPlanetData.PieceId;
        int currentPieces = ItemManager.Instance.GetItem(pieceId);
        int requiredPieces = currentPlanetStarUpgradeData.UpgradeResource;

        if(currentPieces >= requiredPieces)
        {
            upgradeButton.interactable = true;
            upgradeButton.GetComponent<Image>().color = canUpgradeColor;
        }
        else
        {
            upgradeButton.interactable = false;
            upgradeButton.GetComponent<Image>().color = defaultColor;
        }
    }

    private async UniTaskVoid OnUpgradeButtonClicked()
    {
        if(currentPlanetStarUpgradeData == null)
        {
            return;
        }

        if(currentUserPlanetInfo.starLevel >= PlanetManager.Instance.MaxStarLevel)
        {
            return;
        }

        int pieceId = currentPlanetData.PieceId;
        int currentPieces = ItemManager.Instance.GetItem(pieceId);
        int requiredPieces = currentPlanetStarUpgradeData.UpgradeResource;

        if(currentPieces < requiredPieces)
        {
            return;
        }

        ItemManager.Instance.AddItem(pieceId, -requiredPieces);
        await ItemManager.Instance.SaveItemsAsync();

        PlanetManager.Instance.StarUpPlanet(currentPlanetData.Planet_ID);
        await PlanetManager.Instance.SavePlanetsAsync();

        currentPlanetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(currentPlanetData.Planet_ID, currentUserPlanetInfo.starLevel);

        int nextStarLevel = currentUserPlanetInfo.starLevel + 1;
        if(nextStarLevel <= PlanetManager.Instance.MaxStarLevel)
        {
            nextPlanetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(currentPlanetData.Planet_ID, currentUserPlanetInfo.starLevel);
        }
        else
        {
            nextPlanetStarUpgradeData = null;
        }

        if(currentPlanetData.Planet_ID == PlanetManager.Instance.ActivePlanetId)
        {
            PlanetStatManager.Instance.UpdateCurrentPlanetStats();
        }

        UpdateAllUI();

        planetPanelUI.RefreshPlanetPanelUI();
        planetInfoUI.Initialize(currentPlanetData, currentUserPlanetInfo);
    }

}
