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
    [SerializeField] private GameObject abilityOneText;
    [SerializeField] private GameObject abilityTwoText;
    [SerializeField] private GameObject abilityThreeText;

    [SerializeField] private GameObject basePanel;

    private TextMeshProUGUI abilityOneTMP;
    private TextMeshProUGUI abilityTwoTMP;
    private TextMeshProUGUI abilityThreeTMP;

    private void Awake()
    {
        exitBtn.onClick.AddListener(OnExitBtnClicked);
        exitBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());

        abilityOneTMP = abilityOneText.GetComponentInChildren<TextMeshProUGUI>();
        abilityTwoTMP = abilityTwoText.GetComponentInChildren<TextMeshProUGUI>();
        abilityThreeTMP = abilityThreeText.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        exitBtn.onClick.RemoveAllListeners();
    }

    public void Initialize(BuffTowerData data)
    {
        var towerDescData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = towerDescData.TowerName;
        towerDescriptionText.text = towerDescData.TowerDescribe;

        slotText.text = $"적용 칸 수 : {data.SlotNum}칸";

        var specialEffect = DataTableManager.SpecialEffectCombinationTable.Get(data.SpecialEffectCombination_ID);
        string upDownString = "증가";

        if(specialEffect.SpecialEffect1_ID != 0)
        {
            var specialEffectData = DataTableManager.SpecialEffectTable.Get(specialEffect.SpecialEffect1_ID);
            var specialEffectTextId = specialEffectData.SpecialEffectText_ID;
            var specialEffectTextData = DataTableManager.SpecialEffectTextTable.Get(specialEffectTextId);

            var specialEffectName = specialEffectTextData.Name;

            upDownString = specialEffect.SpecialEffect1Value > 0 ? "증가" : "감소";

            var isRate = specialEffectData.SpecialEffectValueType == 1;
            var suffix = isRate ? "%" : "";

            abilityOneTMP.text = $"{specialEffectName} {specialEffect.SpecialEffect1Value}{suffix} {upDownString}";
        }
        else
        {
            abilityOneText.SetActive(false);
        }

        if(specialEffect.SpecialEffect2_ID != 0)
        {
            var specialEffectData = DataTableManager.SpecialEffectTable.Get(specialEffect.SpecialEffect2_ID);
            var specialEffectTextId = specialEffectData.SpecialEffectText_ID;
            var specialEffectTextData = DataTableManager.SpecialEffectTextTable.Get(specialEffectTextId);

            var specialEffectName = specialEffectTextData.Name;
            upDownString = specialEffect.SpecialEffect2Value > 0 ? "증가" : "감소";

            
            var isRate = specialEffectData.SpecialEffectValueType == 1;
            var suffix = isRate ? "%" : "";

            abilityTwoTMP.text = $"{specialEffectName} {specialEffect.SpecialEffect2Value}{suffix} {upDownString}";
        }
        else
        {
            abilityTwoTMP.text = "없음";
        }

        if(specialEffect.SpecialEffect3_ID != 0)
        {
            var specialEffectData = DataTableManager.SpecialEffectTable.Get(specialEffect.SpecialEffect3_ID);
            var specialEffectTextId = specialEffectData.SpecialEffectText_ID;
            var specialEffectTextData = DataTableManager.SpecialEffectTextTable.Get(specialEffectTextId);

            var specialEffectName = specialEffectTextData.Name;
            upDownString = specialEffect.SpecialEffect3Value > 0 ? "증가" : "감소";
            
            var isRate = specialEffectData.SpecialEffectValueType == 1;
            var suffix = isRate ? "%" : "";

            abilityThreeTMP.text = $"{specialEffectName} {specialEffect.SpecialEffect3Value}{suffix} {upDownString}";
        }
        else
        {
            abilityThreeTMP.text = "없음";
        }
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
        basePanel.SetActive(true);
    }
}
