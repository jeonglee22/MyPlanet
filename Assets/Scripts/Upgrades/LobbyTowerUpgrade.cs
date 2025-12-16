using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTowerUpgrade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starDustText;
    [SerializeField] private TextMeshProUGUI goldRequiredText;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TowerUpgradeUI towerUpgradeUI;

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

    private async UniTaskVoid OnClickConfirmButton()
    {
        var currentUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = currentUpgradeData.towerIds.IndexOf(towerId);

        if (towerIndex == -1)
            return;

        var upgradeLevel = currentUpgradeData.upgradeLevels[towerIndex];

        UserData.Gold -= upgradeGold;
        UserData.TowerEnhanceItem -= upgradeStarDust;
        currentUpgradeData.upgradeLevels[towerIndex] = upgradeLevel + 1;
        await UserTowerUpgradeManager.Instance.SaveUserTowerUpgradeAsync(currentUpgradeData);

        Initialize(towerId);

        var upgradeCount = towerUpgradeUI.UpdateTowerUpgradeInfo(currentUpgradeData);
        int totalUpgradePercent = Mathf.FloorToInt((float)upgradeCount / towerUpgradeUI.TotalUpgrade * 100);
        towerUpgradeUI.SetUpgradePercentText(totalUpgradePercent);

        await ItemManager.Instance.SaveItemsAsync();
        await CurrencyManager.Instance.SaveCurrencyAsync();

        confirmPanel.SetActive(false);
    }

    private void OnClickUpgradeButton()
    {
        if (UserData.Gold < upgradeGold)
        {
            Debug.Log("골드 부족");
            return;
        }

        if (UserData.TowerEnhanceItem < upgradeStarDust)
        {
            Debug.Log("스타더스트 부족");
            return;
        }

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

        confirmPanel.SetActive(true);
    }

    public void SetGoldRequiredText(int amount)
    {
        goldRequiredText.text = amount.ToString();
    }

    public void SetStarDustText(int currentAmount, int requiredAmount)
    {
        starDustText.text = $"{currentAmount} / {requiredAmount}";
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
            SetGoldRequiredText(0);
            SetStarDustText(UserData.TowerEnhanceItem, 0);
            return;
        }

        if (upgradeLevel < 0)
            return;

        if (upgradeLevel == 3)
        {
            
        }

        var upgradeDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(towerId, upgradeLevel + 1);
        var upgradeData = DataTableManager.TowerUpgradeTable.Get(upgradeDataId);

        if (upgradeData == null)
            return;

        upgradeGold = upgradeData.GoldCost;
        upgradeStarDust = upgradeData.MaterialCost;

        SetGoldRequiredText(upgradeGold);
        SetStarDustText(UserData.TowerEnhanceItem, upgradeStarDust);
    }
}
