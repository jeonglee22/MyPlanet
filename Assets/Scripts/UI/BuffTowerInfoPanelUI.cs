using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffTowerInfoPanelUI : MonoBehaviour
{
    [SerializeField] private Button exitBtn;

    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerDescriptionText;

    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private TextMeshProUGUI abilityOneText;
    [SerializeField] private TextMeshProUGUI abilityTwoText;
    [SerializeField] private TextMeshProUGUI abilityThreeText;

    private void Start()
    {
        exitBtn.onClick.AddListener(OnExitBtnClicked);
    }

    private void OnDestroy()
    {
        exitBtn.onClick.RemoveListener(OnExitBtnClicked);
    }

    public void Initialize(BuffTowerData data)
    {
        var towerDescData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = data.BuffTowerName;
        towerDescriptionText.text = towerDescData.TowerDescribe;

        slotText.text = $"{data.SlotNum}";

        var upgradeId = data.BuffTowerReinforceUpgrade_ID[0];
        var upgradeData = DataTableManager.BuffTowerReinforceUpgradeTable.GetById(upgradeId);
        
        abilityOneText.text = upgradeData.SpecialEffect1_ID != 0 ? DataTableManager.SpecialEffectTable.Get(upgradeData.SpecialEffect1_ID).SpecialEffectName : "없음";
        abilityTwoText.text = upgradeData.SpecialEffect2_ID != 0 ? DataTableManager.SpecialEffectTable.Get(upgradeData.SpecialEffect2_ID).SpecialEffectName : "없음";
        abilityThreeText.text = upgradeData.SpecialEffect3_ID != 0 ? DataTableManager.SpecialEffectTable.Get(upgradeData.SpecialEffect3_ID).SpecialEffectName : "없음";
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
    }
}
