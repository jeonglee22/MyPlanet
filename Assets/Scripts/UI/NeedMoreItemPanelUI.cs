using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeedMoreItemPanelUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI explainationText;
    [SerializeField] private TextMeshProUGUI gainPositionText;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button stageButton;

    [SerializeField] private GameObject gachaPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject towerInfoPanel;
    [SerializeField] private GameObject totalUpgradePanel;

    private void Start()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        gachaButton.onClick.AddListener(OnGachaButtonClicked);
        shopButton.onClick.AddListener(OnShopButtonClicked);
        stageButton.onClick.AddListener(OnStageButtonClicked);
    }

    private void OffPanels()
    {
        upgradePanel.SetActive(false);
        towerInfoPanel.SetActive(false);
        totalUpgradePanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SetNeedMoreGoldPanel()
    {
        titleText.text = "골드가 부족합니다!";
        explainationText.text = "골드가 부족하여\n강화를 진행할 수 없습니다.";
        gainPositionText.text = "골드 획득처";
    }

    public void SetNeedMoreTowerEnhanceItemPanel()
    {
        titleText.text = "별가루가 부족합니다!";
        explainationText.text = "별가루가 부족하여\n강화를 진행할 수 없습니다.";
        gainPositionText.text = "별가루 획득처";
    }

    public void OnGachaButtonClicked()
    {
        Debug.Log("Gacha Button Clicked");
        OffPanels();
        gachaPanel.SetActive(true);
    }

    public void OnShopButtonClicked()
    {
        Debug.Log("Shop Button Clicked");
        OffPanels();
        shopPanel.SetActive(true);
    }

    public void OnStageButtonClicked()
    {
        Debug.Log("Stage Button Clicked");
        OffPanels();
        lobbyPanel.SetActive(true);
    }
}
