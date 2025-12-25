using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewAmplifierTowerCardUiSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerExplainText;
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI installPointText;
    // [SerializeField] private GameObject specialAbilityObjectBackground;

    [SerializeField] private RectTransform[] contentRoots;
    [SerializeField] private TextMeshProUGUI[] abilityTexts;
    [SerializeField] private TextMeshProUGUI[] abilityValueTexts;

    [SerializeField] private TextMeshProUGUI leftAbilitySlotText;
    [SerializeField] private TextMeshProUGUI rightAbilitySlotText;

    public void SettingNewTowerCard(int towerId, int ability, List<int> leftPoints, List<int> rightPoints)
    {
        var amplifierTowerData = DataTableManager.BuffTowerTable.Get(towerId);

        // var towerName = amplifierTowerData.BuffTowerName;
        var towerExplainId = amplifierTowerData.TowerText_ID;
        var towerExplainText = DataTableManager.TowerExplainTable.Get(towerExplainId).TowerDescribe;
        var towerName = DataTableManager.TowerExplainTable.Get(towerExplainId).TowerName;

        SetTowerName(towerName);
        SetTowerExplain(towerExplainText);

        var buffTowerAssetName = amplifierTowerData.BuffTowerAssetCut;
        var buffTowerAsset = LoadManager.GetLoadedGameTexture(buffTowerAssetName);
        if(buffTowerAsset!=null)
        {
            towerImage.sprite = LoadManager.GetLoadedGameTexture(buffTowerAssetName);
        }

        leftPoints.Sort();
        rightPoints.Sort();

        leftAbilitySlotText.text = $"{string.Join(",", leftPoints)}";
        rightAbilitySlotText.text = $"{string.Join(",", rightPoints)}";

        // installPointText.text = $"왼쪽 : {string.Join(",", leftPoints)}\n오른쪽 : {string.Join(",", rightPoints)}";
        
        var abilityData = DataTableManager.RandomAbilityTable.Get(ability);
        Debug.Log($"[Card] ability={ability}, Effect2_ID={abilityData.SpecialEffect2_ID}, Effect3_ID={abilityData.SpecialEffect3_ID}");
        var specialEffectCombination_ID = amplifierTowerData.SpecialEffectCombination_ID;
        var specialEffectCombinationData = DataTableManager.SpecialEffectCombinationTable.Get(specialEffectCombination_ID);

        var specialEffectID1 = specialEffectCombinationData.SpecialEffect1_ID;
        var specialEffectValue1 = specialEffectCombinationData.SpecialEffect1Value;
        var specialEffectID2 = specialEffectCombinationData.SpecialEffect2_ID;
        var specialEffectValue2 = specialEffectCombinationData.SpecialEffect2Value;
        var specialEffectID3 = specialEffectCombinationData.SpecialEffect3_ID;
        var specialEffectValue3 = specialEffectCombinationData.SpecialEffect3Value;

        int[] specialEffectIDs = { specialEffectID1, specialEffectID2, specialEffectID3 };
        float[] specialEffectValues = { specialEffectValue1, specialEffectValue2, specialEffectValue3 };

        var abilityName = abilityData.RandomAbilityName;
        var abilityValue = abilityData.SpecialEffectValue;

        var index = 0;

        for (int i = 0; i < specialEffectIDs.Length; i++)
        {
            if (specialEffectIDs[i] == 0)
                break;

            if (index >= abilityTexts.Length || index >= abilityValueTexts.Length)
                break;

            var effectDataId = DataTableManager.RandomAbilityTable.GetAbilityIdFromEffectId(specialEffectIDs[i]);
            var effectData = DataTableManager.RandomAbilityTable.Get(effectDataId);
            abilityTexts[index].text = effectData.RandomAbilityName;
            abilityValueTexts[index].text = specialEffectValues[i].ToString();
            index++;
        }

        abilityTexts[index].text = abilityName;
        abilityValueTexts[index].text = abilityValue.ToString();
        index++;

        for (int i = index; i < contentRoots.Length; i++)
        {
            contentRoots[i].gameObject.SetActive(false);
        }
    }

    private void SetTowerName(string towerName)
    {
        towerNameText.text = towerName;
    }

    private void SetTowerExplain(string explain)
    {
        towerExplainText.text = explain;
    }
}
