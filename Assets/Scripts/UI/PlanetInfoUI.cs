using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetInfoUI : MonoBehaviour
{
    [SerializeField] private Button homeBtn;

    [SerializeField] private Image planetIcon;
    [SerializeField] private TextMeshProUGUI planetLevelText;
    [SerializeField] private List<GameObject> upgradeStars;
    [SerializeField] private Slider pieceSlider;
    [SerializeField] private Sprite changePieceImage;
    [SerializeField] private Sprite defaultPieceImage;
    [SerializeField] private TextMeshProUGUI pieceText;
    [SerializeField] private Button starUpgradeBtn;

    [SerializeField] private TextMeshProUGUI planetNameText;
    [SerializeField] private TextMeshProUGUI fightingPowerText;
    [SerializeField] private TextMeshProUGUI descText;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private TextMeshProUGUI expRateText;
    [SerializeField] private TextMeshProUGUI drainText;
    [SerializeField] private TextMeshProUGUI healthRegenerationText;
    [SerializeField] private Button levelUpBtn;

    [SerializeField] private Button saveBtn;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color canUpgradeColor;

    [SerializeField] private List<GameObject> unSelectIcon;
    [SerializeField] private List<GameObject> unlockIcon;

    [SerializeField] private GameObject outLineImage;
    [SerializeField] private PlanetPanelUI planetPanelUI;

    private PlanetData currentPlanetData;
    private UserPlanetInfo currentUserPlanetInfo;
    private PlanetLvUpgradeData planetLvUpgradeData;
    private PlanetStarUpgradeData planetStarUpgradeData;

    private void Awake()
    {
        for(int i = 0; i < unSelectIcon.Count; i++)
        {
            int index = i;
            var btn = unSelectIcon[index].GetComponent<Button>();
            btn.onClick.AddListener(() => OnPlanetBtnClick(index));
            btn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        }
    }

    private void OnEnable()
    {
        homeBtn.gameObject.SetActive(true);
        saveBtn.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        homeBtn.gameObject.SetActive(false);
        saveBtn.gameObject.SetActive(false);
    }

    public void Initialize(PlanetData planetData, UserPlanetInfo userPlanetInfo)
    {
        if(PlanetManager.Instance.ActivePlanetId == planetData.Planet_ID)
        {
            outLineImage.SetActive(true);
        }
        else
        {
            outLineImage.SetActive(false);
        }

        int selectIdx = planetData.Planet_ID - 300001;
        for(int i = 0; i < unSelectIcon.Count; i++)
        {
            int checkPlanetId = 300001 + i;
            var checkUserPlanetInfo = PlanetManager.Instance.GetPlanetInfo(checkPlanetId);

            unSelectIcon[i].SetActive(i != selectIdx);

            if(i < unlockIcon.Count)
            {
                unlockIcon[i].SetActive(!checkUserPlanetInfo.owned);
            }

            var btn = unSelectIcon[i].GetComponent<Button>();
            if(btn != null)
            {
                btn.interactable = checkUserPlanetInfo.owned;
            }
        }

        currentPlanetData = planetData;
        currentUserPlanetInfo = userPlanetInfo;
        var planetTextData = DataTableManager.PlanetTextTable.Get(planetData.PlanetText_ID);

        UpdateFightingPower();
        UpdateStatsUI();

        planetNameText.text = planetTextData.PlanetName;
        descText.text = planetTextData.PlanetDescribe;

        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetImage);

        if(planetData.Planet_ID == (int)PlanetType.BasePlanet)
        {
            planetLevelText.text = $"Lv. 0";

            pieceText.text = $"MAX";

            for(int i = 1; i < upgradeStars.Count; i++)
            {
                upgradeStars[i].SetActive(false);
            }

            pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;

            levelUpBtn.interactable = false;
            starUpgradeBtn.interactable = false;
            pieceSlider.maxValue = 1;
            pieceSlider.value = 1;

            return;
        }

        int level = userPlanetInfo.level + 1 > PlanetManager.Instance.MaxLevel ? PlanetManager.Instance.MaxLevel : userPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, level);

        planetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, userPlanetInfo.starLevel + 1);

        planetLevelText.text = $"Lv. {userPlanetInfo.level}";

        for (int i = 0; i < upgradeStars.Count; i++)
        {
            upgradeStars[i].SetActive(i < userPlanetInfo.starLevel);
        }

        UpdatePieceSlider(planetData);
        UpdateLevelUpButton();

        levelUpBtn.interactable = true;
        starUpgradeBtn.interactable = true;

    }

    private void UpdatePieceSlider(PlanetData planetData)
    {
        int pieceId = planetData.PieceId;

        int currentPieces = ItemManager.Instance.GetItem(pieceId);

        int requiredPieces = planetStarUpgradeData?.UpgradeResource ?? 0;

        if(currentPieces >= requiredPieces)
        {
            pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;
            starUpgradeBtn.gameObject.GetComponent<Image>().color = canUpgradeColor;
        }
        else
        {
            pieceSlider.fillRect.GetComponent<Image>().sprite = defaultPieceImage;
            starUpgradeBtn.gameObject.GetComponent<Image>().color = defaultColor;
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

        fightingPowerText.text = FormatStat(cal);
    }

    private void UpdateStatsUI()
    {
        var currentStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
        currentPlanetData.Planet_ID, 
        currentUserPlanetInfo.level, 
        currentUserPlanetInfo.starLevel);

        healthText.text = FormatStat(currentStats.hp);
        defenseText.text = FormatStat(currentStats.defense);
        shieldText.text = FormatStat(currentStats.shield);
        expRateText.text = FormatStat(currentStats.expRate);
        drainText.text = FormatStat(currentStats.drain);
        healthRegenerationText.text = FormatStat(currentStats.hpRegeneration);
    }

    private void UpdateLevelUpButton()
    {
        if(UserData.Gold < planetLvUpgradeData.UpgradeResource)
        {
            levelUpBtn.gameObject.GetComponent<Image>().color = defaultColor;
        }
        else
        {
            levelUpBtn.gameObject.GetComponent<Image>().color = canUpgradeColor;
        }
    }

    private void OnPlanetBtnClick(int iconIndex)
    {
        int planetId = 300001 + iconIndex;

        var planetData = DataTableManager.PlanetTable.Get(planetId);
        var userPlanetInfo = PlanetManager.Instance.GetPlanetInfo(planetId);

        Initialize(planetData, userPlanetInfo);

        if(planetPanelUI != null)
        {
            planetPanelUI.SetChoosedIndex(iconIndex + 1);
        }
    }

    private string FormatStat(float value)
    {
        return value % 1 == 0 ? $"{value:F0}" : $"{value:F1}";
    }
}
