using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
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
        currentPlanetData = planetData;
        currentUserPlanetInfo = userPlanetInfo;
        var planetTextData = DataTableManager.PlanetTextTable.Get(planetData.PlanetText_ID);

        UpdateFightingPower();
        UpdateStatsUI();

        planetNameText.text = planetTextData.PlanetName;
        descText.text = planetTextData.PlanetDescribe;

        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetIcon);

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

            return;
        }

        int level = userPlanetInfo.level + 1 > PlanetManager.Instance.MaxLevel ? PlanetManager.Instance.MaxLevel : userPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, level);

        int starLevel = userPlanetInfo.starLevel + 1 > PlanetManager.Instance.MaxStarLevel ? PlanetManager.Instance.MaxStarLevel : userPlanetInfo.starLevel + 1;

        planetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, starLevel);

        planetLevelText.text = $"Lv. {userPlanetInfo.level}";

        for (int i = 0; i < upgradeStars.Count; i++)
        {
            upgradeStars[i].SetActive(i < userPlanetInfo.starLevel);
        }

        UpdatePieceSlider(planetData);
        UpdateLevelUpButton();

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
            starUpgradeBtn.interactable = true;
        }
        else
        {
            pieceSlider.fillRect.GetComponent<Image>().sprite = defaultPieceImage;
            starUpgradeBtn.gameObject.GetComponent<Image>().color = defaultColor;
            starUpgradeBtn.interactable = false;
        }

        pieceSlider.maxValue = requiredPieces;
        pieceSlider.value = currentPieces;

        pieceText.text = $"{currentPieces} / {requiredPieces}";
    }

    private void UpdateFightingPower()
    {
        PlanetStats baseStats = PlanetStatManager.Instance.GetBasePlanetStats(currentPlanetData.Planet_ID);
        var cal = (baseStats.hp * (100 + baseStats.defense) / 100f) + baseStats.shield + (baseStats.hpRegeneration * 420) + (baseStats.drain * 650);
        fightingPowerText.text = $"{(int)cal}";
    }

    private void UpdateStatsUI()
    {
        var baseStats = PlanetStatManager.Instance.GetBasePlanetStats(currentPlanetData.Planet_ID);

        healthText.text = $"{baseStats.hp}";
        defenseText.text = $"{baseStats.defense}";
        shieldText.text = $"{baseStats.shield}";
        expRateText.text = $"{baseStats.expRate}%";
        drainText.text = $"{baseStats.drain}";
        healthRegenerationText.text = $"{baseStats.hpRegeneration}";
    }

    private void UpdateLevelUpButton()
    {
        if(UserData.Gold < planetLvUpgradeData.UpgradeResource)
        {
            levelUpBtn.interactable = false;
            levelUpBtn.gameObject.GetComponent<Image>().color = defaultColor;
        }
        else
        {
            levelUpBtn.interactable = true;
            levelUpBtn.gameObject.GetComponent<Image>().color = canUpgradeColor;
        }
    }
}
