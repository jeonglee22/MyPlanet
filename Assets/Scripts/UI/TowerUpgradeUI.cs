using System;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBackBtnClicked()
    {
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
