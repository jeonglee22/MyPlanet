using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
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
    [SerializeField] private Button lockOpenButton;
    [SerializeField] private GameObject outLineImage;

    private PlanetStarUpgradeData planetStarUpgradeData;
    private PlanetData currentPlanetData;
    private UserPlanetInfo currentUserPlanetInfo;

    private void Start()
    {
        lockOpenButton.onClick.AddListener(() => OnLockOpenBtnClicked().Forget());
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

        currentPlanetData = planetData;
        currentUserPlanetInfo = userPlanetInfo;

        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetImage);
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
            pieceSlider.maxValue = 1;
            pieceSlider.value = 1;

            lockOpenButton.gameObject.SetActive(false);

            return;
        }

        if(userPlanetInfo.owned == false)
        {
            notOwnImage.SetActive(true);
            blackImage.SetActive(true);

            planetLevel.text = $"";
            planetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, 1);

            itemButton.interactable = false;

            UpdateLockOpenButton();
        }
        else
        {
            notOwnImage.SetActive(false);
            blackImage.SetActive(false);

            planetLevel.text = $"Lv. {userPlanetInfo.level}";

            int nextStarLevel = userPlanetInfo.starLevel + 1;

            if(nextStarLevel > PlanetManager.Instance.MaxStarLevel)
            {
                pieceText.text = $"MAX";
                pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;
            }
            else
            {
                planetStarUpgradeData = DataTableManager.PlanetStarUpgradeTable.GetCurrentLevelData(planetData.Planet_ID, nextStarLevel);

                for (int i = 0; i < upgradeStar.Count; i++)
                {
                    upgradeStar[i].SetActive(i < userPlanetInfo.starLevel);
                }
            }

            itemButton.interactable = true;

            lockOpenButton.gameObject.SetActive(false);
        }

        UpdatePieceSlider(planetData, currentUserPlanetInfo.starLevel);

    }

    private void UpdatePieceSlider(PlanetData planetData, int starLevel)
    {
        if(starLevel == PlanetManager.Instance.MaxStarLevel)
        {
            pieceText.text = $"MAX";
            pieceSlider.fillRect.GetComponent<Image>().sprite = changePieceImage;
            pieceSlider.maxValue = 1;
            pieceSlider.value = 1;
            return;
        }

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

    private async UniTaskVoid OnLockOpenBtnClicked()
    {
        if (currentUserPlanetInfo.owned)
        {
            return;
        }

        int pieceId = currentPlanetData.PieceId;
        int currentPieces = ItemManager.Instance.GetItem(pieceId);
        int requiredPieces = planetStarUpgradeData.UpgradeResource;

        if(currentPieces < requiredPieces)
        {
            Debug.Log("[PlanetItemUI] 행성 조각이 부족합니다.");
            return;
        }

        ItemManager.Instance.AddItem(pieceId, -requiredPieces);
        PlanetManager.Instance.UnlockPlanet(currentPlanetData.Planet_ID);

        var planetPanelUI = GameObject.FindGameObjectWithTag(TagName.PlanetPanelUI).GetComponent<PlanetPanelUI>();
        planetPanelUI.RefreshPlanetPanelUI();

        SaveUnlockDataAsync().Forget();
    }

    private async UniTaskVoid SaveUnlockDataAsync()
    {
        try
        {
            await ItemManager.Instance.SaveItemsAsync();
            await PlanetManager.Instance.SavePlanetsAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PlanetItemUI] 행성 잠금 해제 데이터 저장 중 오류 발생: {ex.Message}");
        }
    }

    public void RefreshUI()
    {
        if(currentPlanetData != null && currentUserPlanetInfo != null)
        {
            Initialize(currentPlanetData, currentUserPlanetInfo);
        }
    }

    private void UpdateLockOpenButton()
    {
        int pieceId = currentPlanetData.PieceId;

        int currentPieces = ItemManager.Instance.GetItem(pieceId);

        int requiredPieces = planetStarUpgradeData.UpgradeResource;

        if(currentPieces >= requiredPieces)
        {
            lockOpenButton.gameObject.SetActive(true);
        }
        else
        {
            lockOpenButton.gameObject.SetActive(false);
        }
    }
}
