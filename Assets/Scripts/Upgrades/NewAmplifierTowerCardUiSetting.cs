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

    public void SettingNewTowerCard(int towerId, int ability, List<int> leftPoints, List<int> rightPoints)
    {
        var amplifierTowerData = DataTableManager.BuffTowerTable.Get(towerId);

        var towerName = amplifierTowerData.BuffTowerName;
        var towerExplainId = amplifierTowerData.TowerText_ID;
        var towerExplainText = DataTableManager.TowerExplainTable.Get(towerExplainId).TowerDescribe;

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

        installPointText.text = $"왼쪽 : {string.Join(",", leftPoints)}\n오른쪽 : {string.Join(",", rightPoints)}";
        
        var abilityData = DataTableManager.RandomAbilityTable.Get(ability);



        // var specialEffectName = abilityData.RandomAbilityName;
        // var specialEffectValue = abilityData.SpecialEffectValue;
        // var randomAbilityObj1 = Instantiate(randomAbilityObject, contentRoot);
        // var abilityTexts = randomAbilityObj1.GetComponentsInChildren<TextMeshProUGUI>();
        
        // if (abilityTexts.Length != 2)
        //     return;

        // abilityTexts[0].text = specialEffectName;
        // abilityTexts[1].text = specialEffectValue.ToString();

        // if (abilityData.SpecialEffect2_ID == 0)
        //     return;
        
        // var specialEffectName2 = abilityData.RandomAbility2Name;
        // var specialEffectValue2 = abilityData.SpecialEffect2Value;
        // var randomAbilityObj2 = Instantiate(randomAbilityObject, contentRoot);
        // var abilityTexts2 = randomAbilityObj2.GetComponentsInChildren<TextMeshProUGUI>();
        // if (abilityTexts2.Length != 2)
        //     return;

        // abilityTexts2[0].text = specialEffectName2;
        // abilityTexts2[1].text = specialEffectValue2.ToString();

        // if (abilityData.SpecialEffect3_ID == 0)
        //     return;
        
        // var specialEffectName3 = abilityData.RandomAbility3Name;
        // var specialEffectValue3 = abilityData.SpecialEffect3Value;
        // var randomAbilityObj3 = Instantiate(randomAbilityObject, contentRoot);
        // var abilityTexts3 = randomAbilityObj3.GetComponentsInChildren<TextMeshProUGUI>();
        // if (abilityTexts3.Length != 2)
        //     return;

        // abilityTexts3[0].text = specialEffectName3;
        // abilityTexts3[1].text = specialEffectValue3.ToString();
    }

    private void SetTowerName(string towerName)
    {
        towerNameText.text = towerName;
    }

    private void SetTowerExplain(string explain)
    {
        towerExplainText.text = explain;
    }

    private RandomAbilityData GetRandomAbilityDataFromTowerId(int towerId)
    {
        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var projectileId = towerData.Projectile_ID;
        var projectileData = DataTableManager.ProjectileTable.Get(projectileId);
        var projectileSpecialEffectId = projectileData.ProjectileProperties1_ID;
        var proejctileSpecialEffectData = DataTableManager.SpecialEffectTable.Get(projectileSpecialEffectId);
        var abilityId = DataTableManager.RandomAbilityTable.GetAbilityIdFromEffectId(projectileSpecialEffectId);
        var abilityData = DataTableManager.RandomAbilityTable.Get(abilityId);

        return abilityData;
    }
}
