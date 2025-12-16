using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject towerInfoPanel;

    [SerializeField] private Button backBtn;
    [SerializeField] private Button[] towerButtons;

    [SerializeField] private TextMeshProUGUI[] towerLevelTexts;
    [SerializeField] private GameObject[] upgradeEnabledObjects;

    [SerializeField] private TextMeshProUGUI totalUpgradePercentText;

    [SerializeField] private TextMeshProUGUI diamondText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI towerNameText;
    private int totalUpgrade;
    public int TotalUpgrade => totalUpgrade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backBtn.onClick.AddListener(OnBackBtnClicked);
        for (int i = 0; i < towerButtons.Length; i++)
        {
            int index = i;
            towerButtons[i].onClick.AddListener(() => OnTowerButtonClicked(index));
        }
    }

    private void OnEnable()
    {
        var currentUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;

        totalUpgrade = currentUpgradeData.towerIds.Count * 4;
        var upgradedCount = UpdateTowerUpgradeInfo(currentUpgradeData);

        // Update total upgrade percent
        int totalUpgradePercent = Mathf.FloorToInt((float)upgradedCount / totalUpgrade * 100);
        SetUpgradePercentText(totalUpgradePercent);

        // Update currency texts
        diamondText.text = (UserData.ChargedDia + UserData.FreeDia).ToString();
        goldText.text = UserData.Gold.ToString();
    }

    public int UpdateTowerUpgradeInfo(UserTowerUpgradeData currentUpgradeData)
    {
        var upgradedCount = 0;
        // Update tower levels
        for (int i = 0; i < towerLevelTexts.Length; i++)
        {
            var attackTowerTableRowId = i switch
            {
                0 => AttackTowerId.basicGun,
                1 => AttackTowerId.Gattling,
                2 => AttackTowerId.Missile,
                3 => AttackTowerId.ShootGun,
                4 => AttackTowerId.Sniper,
                5 => AttackTowerId.Lazer,
                _ => throw new ArgumentOutOfRangeException(nameof(i), "Invalid tower button index")
            };

            var towerIndex = currentUpgradeData.towerIds.IndexOf((int)attackTowerTableRowId);
            if (towerIndex == -1)
                continue;
            
            var attackTowerLevel = currentUpgradeData.upgradeLevels[towerIndex];

            if (attackTowerLevel >= 4)
            {
                towerLevelTexts[i].text = "Max Lv.";
            }
            else
            {
                towerLevelTexts[i].text = $"Lv. {attackTowerLevel}";
            }
            CheckUpgradeAvailability(attackTowerLevel, i);
            upgradedCount += attackTowerLevel;
        }

        return upgradedCount;
    }

    private void OnTowerButtonClicked(int index)
    {
        var attackTowerTableRowId = index switch
        {
            0 => AttackTowerId.basicGun,
            1 => AttackTowerId.Gattling,
            2 => AttackTowerId.Missile,
            3 => AttackTowerId.ShootGun,
            4 => AttackTowerId.Sniper,
            5 => AttackTowerId.Lazer,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid tower button index")
        };

        towerInfoPanel.SetActive(true);
        towerInfoPanel.GetComponent<TowerInfoPanelUI>().Initialize(
            DataTableManager.AttackTowerTable.GetById((int)attackTowerTableRowId));

        towerInfoPanel.GetComponent<LobbyTowerUpgrade>().Initialize((int)attackTowerTableRowId);
    }

    public void OnBackBtnClicked()
    {
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void CheckUpgradeAvailability(int towerLevel, int index)
    {
        var attackTowerTableRowId = index switch
        {
            0 => AttackTowerId.basicGun,
            1 => AttackTowerId.Gattling,
            2 => AttackTowerId.Missile,
            3 => AttackTowerId.ShootGun,
            4 => AttackTowerId.Sniper,
            5 => AttackTowerId.Lazer,
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid tower button index")
        };

        if (towerLevel >= 4)
        {
            upgradeEnabledObjects[index].SetActive(false);
            return;
        }

        if (towerLevel == 3)
        {
            var abilityUnlockUpgradeDataId = DataTableManager.TowerUpgradeAbilityUnlockTable.GetDataId((int)attackTowerTableRowId);
            var abilityUnlockData = DataTableManager.TowerUpgradeAbilityUnlockTable.Get(abilityUnlockUpgradeDataId);
            if (abilityUnlockData == null)
            {
                upgradeEnabledObjects[index].SetActive(false);
                return;
            }
            
            upgradeEnabledObjects[index].SetActive(CanUpgrade(abilityUnlockData.GoldCost, abilityUnlockData.MaterialCost));
            return;
        }

        var towerUpgradeId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount((int)attackTowerTableRowId, towerLevel + 1);
        
        if (towerUpgradeId == -1)
        {
            return;
        }

        var towerUpgradeData = DataTableManager.TowerUpgradeTable.Get(towerUpgradeId);
        if (towerUpgradeData == null)
        {
            return;
        }

        upgradeEnabledObjects[index].SetActive(CanUpgrade(towerUpgradeData.GoldCost, towerUpgradeData.MaterialCost));
        // var 
    }

    private bool CanUpgrade(int goldCost, int materialCost)
    {
        var currentGold = UserData.Gold;
        var currentStarDust = UserData.TowerEnhanceItem;
        bool canUpgrade = currentGold >= goldCost && currentStarDust >= materialCost;

        return canUpgrade;
    }

    public void SetUpgradePercentText(int percent)
    {
        totalUpgradePercentText.text = $"Total Upgrade: {percent}%";
    }
}
