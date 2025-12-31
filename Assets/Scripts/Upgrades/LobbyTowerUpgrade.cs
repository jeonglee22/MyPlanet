using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif
public class LobbyTowerUpgrade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starDustText;
    [SerializeField] private TextMeshProUGUI goldRequiredText;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject needMoreItemPanel;
    [SerializeField] private TowerUpgradeUI towerUpgradeUI;
    [SerializeField] private TowerInfoPanelUI towerInfoPanelUI;


    [SerializeField] private Slider starDustSlider;

    private int towerId;

    private int upgradeGold;
    private int upgradeStarDust;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        cancelButton.onClick.AddListener(() => confirmPanel.SetActive(false));
        confirmButton.onClick.AddListener(() => OnClickConfirmButton().Forget());
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
        {
            UserData.TowerEnhanceItem += 100000;
            ItemManager.Instance.SaveItemsAsync().Forget();
            CurrencyManager.Instance.SaveCurrencyAsync().Forget();
            Debug.Log($"[DEBUG] Stardust +100000 => {UserData.TowerEnhanceItem}");
            Initialize(towerId);
        }
    }
#endif

    private void OnDisable()
    {
        confirmPanel.SetActive(false);
    }

    private async UniTaskVoid OnClickConfirmButton()
    {
        var currentUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = currentUpgradeData.towerIds.IndexOf(towerId);

        if (towerIndex == -1)
            return;

        var upgradeLevel = currentUpgradeData.upgradeLevels[towerIndex];

        if (upgradeLevel >= 4)
        {
            Debug.Log("최대 레벨 도달");
            return;
        }

        if (upgradeLevel == 3)
        {
            var abilityUnlockUpgradeDataId = DataTableManager.TowerUpgradeAbilityUnlockTable.GetDataId(towerId);
            var abilityUnlockData = DataTableManager.TowerUpgradeAbilityUnlockTable.Get(abilityUnlockUpgradeDataId);
            if (abilityUnlockData == null)
                return;

            UserData.Gold -= upgradeGold;
            UserData.TowerEnhanceItem -= upgradeStarDust;
            
            currentUpgradeData.upgradeLevels[towerIndex] = upgradeLevel + 1;

            // Unlock new ability logic can be added here

            await UserTowerUpgradeManager.Instance.SaveUserTowerUpgradeAsync(currentUpgradeData);

            await UpdateUIs(currentUpgradeData);

            return;
        }

        UserData.Gold -= upgradeGold;
        UserData.TowerEnhanceItem -= upgradeStarDust;
        currentUpgradeData.upgradeLevels[towerIndex] = upgradeLevel + 1;
        await UserTowerUpgradeManager.Instance.SaveUserTowerUpgradeAsync(currentUpgradeData);

        await UpdateUIs(currentUpgradeData);
    }

    private async UniTask UpdateUIs(UserTowerUpgradeData currentUpgradeData)
    {
        Initialize(towerId);
        towerInfoPanelUI.InitializeUpgrade(DataTableManager.AttackTowerTable.GetById(towerId));

        var upgradeCountFull = towerUpgradeUI.UpdateTowerUpgradeInfo(currentUpgradeData);
        int totalUpgradePercentFull = Mathf.FloorToInt((float)upgradeCountFull / towerUpgradeUI.TotalUpgrade * 100);
        towerUpgradeUI.SetUpgradePercentText(totalUpgradePercentFull);

        await ItemManager.Instance.SaveItemsAsync();
        await CurrencyManager.Instance.SaveCurrencyAsync();

        confirmPanel.SetActive(false);

        await UserAttackPowerManager.Instance.UpdateTowerPower();
    }

    private void OnClickUpgradeButton()
    {
        var currentUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = currentUpgradeData.towerIds.IndexOf(towerId);

        if (towerIndex == -1)
            return;

        var upgradeLevel = currentUpgradeData.upgradeLevels[towerIndex];
        if (upgradeLevel >= 4)
        {
            Debug.Log("최대 레벨 도달");
            return;
        }

        if (UserData.Gold < upgradeGold)
        {
            Debug.Log("골드 부족");
            needMoreItemPanel.SetActive(true);
            var needMoreItemPanelUi = needMoreItemPanel.GetComponent<NeedMoreItemPanelUI>();
            needMoreItemPanelUi.SetNeedMoreGoldPanel();
            return;
        }

        if (UserData.TowerEnhanceItem < upgradeStarDust)
        {
            Debug.Log("스타더스트 부족");
            needMoreItemPanel.SetActive(true);
            var needMoreItemPanelUi = needMoreItemPanel.GetComponent<NeedMoreItemPanelUI>();
            needMoreItemPanelUi.SetNeedMoreTowerEnhanceItemPanel();
            return;
        }

        confirmPanel.SetActive(true);
    }

    public void SetGoldRequiredText(int amount)
    {
        goldRequiredText.text = amount.ToString();
    }

    public void SetGoldRequiredTextMaxLevel()
    {
        goldRequiredText.text = "MAX";
    }

    public void SetStarDustText(int currentAmount, int requiredAmount)
    {
        starDustText.text = $"{currentAmount} / {requiredAmount}";
    }

    public void SetStarDustMaxLevelText()
    {
        starDustText.text = "MAX";
    }

    public void Initialize(int index)
    {
        towerId = index;

        var currentUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = currentUpgradeData.towerIds.IndexOf(towerId);

        if (towerIndex == -1)
            return;

        var upgradeLevel = currentUpgradeData.upgradeLevels[towerIndex];

        if (upgradeLevel >= 4)
        {
            SetGoldRequiredTextMaxLevel();
            SetStarDustMaxLevelText();
            starDustSlider.value = 1f;
            return;
        }

        if (upgradeLevel < 0)
            return;

        if (upgradeLevel == 3)
        {
            var abilityUnlockUpgradeDataId = DataTableManager.TowerUpgradeAbilityUnlockTable.GetDataId(towerId);
            var abilityUnlockData = DataTableManager.TowerUpgradeAbilityUnlockTable.Get(abilityUnlockUpgradeDataId);
            if (abilityUnlockData == null)
                return;

            upgradeGold = abilityUnlockData.GoldCost;
            upgradeStarDust = abilityUnlockData.MaterialCost;
            SetGoldRequiredText(upgradeGold);
            SetStarDustText(UserData.TowerEnhanceItem, upgradeStarDust);

            var ratioLevel3 = (float)UserData.TowerEnhanceItem / upgradeStarDust;

            starDustSlider.value = Mathf.Clamp01(ratioLevel3);
            Debug.Log("슬라이더 값 설정: " + starDustSlider.value);
            SetStarDustSliderValue(starDustSlider.value);
            return;
        }

        var upgradeDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(towerId, upgradeLevel + 1);
        var upgradeData = DataTableManager.TowerUpgradeTable.Get(upgradeDataId);

        if (upgradeData == null)
            return;

        upgradeGold = upgradeData.GoldCost;
        upgradeStarDust = upgradeData.MaterialCost;

        SetGoldRequiredText(upgradeGold);
        SetStarDustText(UserData.TowerEnhanceItem, upgradeStarDust);

        var ratio = (float)UserData.TowerEnhanceItem / upgradeStarDust;

        starDustSlider.value = Mathf.Clamp01(ratio);
        Debug.Log("슬라이더 값 설정: " + starDustSlider.value);
        SetStarDustSliderValue(starDustSlider.value);
    }

    public void SetStarDustSliderValue(float value)
    {
        starDustSlider.value = value;
    }
}