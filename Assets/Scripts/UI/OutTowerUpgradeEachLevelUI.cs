using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutTowerUpgradeEachLevelUI : MonoBehaviour
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityNameText;

    [SerializeField] private TextMeshProUGUI currentStatText;
    [SerializeField] private TextMeshProUGUI nextStatText;

    [SerializeField] private List<Image> abilityIcons;
    [SerializeField] private List<TextMeshProUGUI> abilityNameTexts;
    [SerializeField] private List<TextMeshProUGUI> newAbilitystatTexts;

    public void SettingUpgradeUiInfo(Sprite icon, string abilityName, string currentStat, string nextStat)
    {
        abilityIcon.sprite = icon;
        abilityNameText.text = abilityName;
        currentStatText.text = currentStat;
        nextStatText.text = nextStat;
    }

    // public void SettingUpgradeUiInfo(int abilityId)
    // {
    //     var abilityData = DataTableManager.RandomAbilityTable.Get(abilityId);
    //     var first

    //     abilityIcon.sprite = icon;
    //     abilityNameText.text = abilityName;
    //     nextStatText.text = newAbilityStat;
    // }

    public void SettingUpgradeUiInfoAll(List<Sprite> icons, List<int> effectIds, List<float> newAbilityStats)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            abilityIcons[i].sprite = icons[i];

            var effectData = DataTableManager.SpecialEffectTable.Get(effectIds[i]);
            var effectTextId = effectData.SpecialEffectText_ID;
            var effectTextData = DataTableManager.SpecialEffectTextTable.Get(effectTextId);
            var effectTextName = effectTextData.Name;
            abilityNameTexts[i].text = effectTextName;
            
            var specialEffectValueType = effectData.SpecialEffectValueType;
            if (specialEffectValueType == 1) // percentage
                newAbilitystatTexts[i].text = $"{newAbilityStats[i]}%";
            else // absolute
                newAbilitystatTexts[i].text = newAbilityStats[i].ToString();

            abilityIcons[i].gameObject.SetActive(true);
            abilityNameTexts[i].gameObject.SetActive(true);
            newAbilitystatTexts[i].gameObject.SetActive(true);
        }

        for (int j = icons.Count; j < abilityIcons.Count; j++)
        {
            abilityIcons[j].gameObject.SetActive(false);
            abilityNameTexts[j].gameObject.SetActive(false);
            newAbilitystatTexts[j].gameObject.SetActive(false);
        }
    }
    
}
