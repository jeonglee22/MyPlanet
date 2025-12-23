using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetItemUI : MonoBehaviour
{
    [SerializeField] private Image planetIcon;
    [SerializeField] private TextMeshProUGUI planetLevel;
    [SerializeField] private List<GameObject> upgradeStar;
    [SerializeField] private Slider pieceSlider;
    [SerializeField] private TextMeshProUGUI pieceText;
    [SerializeField] private Sprite changePieceImage;
    [SerializeField] private Sprite defaultPieceImage;
    [SerializeField] private Button itemButton; 
    [SerializeField] private GameObject notOwnImage;
    [SerializeField] private GameObject blackImage;

    private PlanetLvUpgradeData planetLvUpgradeData;
    private PlanetStarUpgradeData planetStarUpgradeData;

    public void Initialize(PlanetData planetData, UserPlanetInfo userPlanetInfo)
    {
        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetIcon);
        if(planetData.Planet_ID == (int)PlanetType.BasePlanet)
        {
            notOwnImage.SetActive(false);
            blackImage.SetActive(false);

            planetLevel.text = $"Lv. 0";

            pieceText.text = $"MAX";

            for(int i = 1; i < upgradeStar.Count; i++)
            {
                upgradeStar[i].SetActive(false);
            }

            pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;

            return;
        }

        int level = userPlanetInfo.level + 1 > PlanetManager.Instance.MaxLevel ? PlanetManager.Instance.MaxLevel : userPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, level);

        if(userPlanetInfo.owned == false)
        {
            notOwnImage.SetActive(true);
            blackImage.SetActive(true);

            planetLevel.text = $"";
            planetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, userPlanetInfo.starLevel + 2);

            itemButton.interactable = false;
        }
        else
        {
            notOwnImage.SetActive(false);
            blackImage.SetActive(false);

            int starLevel = userPlanetInfo.starLevel + 1 > PlanetManager.Instance.MaxStarLevel ? PlanetManager.Instance.MaxStarLevel : userPlanetInfo.starLevel + 1;

            planetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, starLevel);

            planetLevel.text = $"Lv. {userPlanetInfo.level}";

            for (int i = 0; i < upgradeStar.Count; i++)
            {
                upgradeStar[i].SetActive(i < userPlanetInfo.starLevel);
            }

            itemButton.interactable = true;
        }

        UpdatePieceSlider(planetData);

    }

    private void UpdatePieceSlider(PlanetData planetData)
    {
        int pieceId = planetData.PieceId;

        int currentPieces = ItemManager.Instance.GetItem(pieceId);

        int requiredPieces = planetStarUpgradeData?.UpgradeResource ?? 0;

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
}
