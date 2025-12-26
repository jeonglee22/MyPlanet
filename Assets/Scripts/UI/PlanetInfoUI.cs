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

    private PlanetData currentPlanetData;
    private UserPlanetInfo currentUserPlanetInfo;
    private PlanetLvUpgradeData planetLvUpgradeData;
    private PlanetStarUpgradeData planetStarUpgradeData;

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
        int selectIdx = planetData.Planet_ID - 300001;
        for(int i = 0; i < unSelectIcon.Count; i++)
        {
            unSelectIcon[i].SetActive(i != selectIdx);
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

        fightingPowerText.text = $"{Mathf.RoundToInt(cal)}";
    }

    private void UpdateStatsUI()
    {
        var currentStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
        currentPlanetData.Planet_ID, 
        currentUserPlanetInfo.level, 
        currentUserPlanetInfo.starLevel);

    healthText.text = $"{Mathf.RoundToInt(currentStats.hp)}";
    defenseText.text = $"{Mathf.RoundToInt(currentStats.defense)}";
    shieldText.text = $"{Mathf.RoundToInt(currentStats.shield)}";
    expRateText.text = $"{Mathf.RoundToInt(currentStats.expRate)}%";
    drainText.text = $"{Mathf.RoundToInt(currentStats.drain)}";
    healthRegenerationText.text = $"{Mathf.RoundToInt(currentStats.hpRegeneration)}";
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
}
