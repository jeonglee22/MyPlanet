using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewAttackTowerCardUiSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerExplainText;
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI specialAbilityText;
    [SerializeField] private GameObject specialAbilityObjectBackground;
    [SerializeField] private GameObject newTowerTextObject;

    [SerializeField] private GameObject randomAbilityObject;

    [SerializeField] private RectTransform contentRoot;

    [SerializeField] private List<GameObject> abilityPanels;
    [SerializeField] private List<GameObject> selfAbilityPanels;

    void OnEnable()
    {
        specialAbilityObjectBackground.SetActive(true);
        specialAbilityText.gameObject.SetActive(true);
    }

    public void SettingNewTowerCard(int towerId, int ability)
    {
        foreach (var panel in selfAbilityPanels)
        {
            panel.SetActive(false);
        }
        foreach (var panel in abilityPanels)
        {
            panel.SetActive(false);
        }

        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var towerExplainId = towerData.TowerText_ID;
        var towerExplainData = DataTableManager.TowerExplainTable.Get(towerExplainId);
        var towerExplainText = towerExplainData.TowerDescribe;
        var towerName = towerExplainData.TowerName;

        SetTowerName(towerName);
        SetTowerExplain(towerExplainText);

        var attackTowerAssetName = towerData.AttackTowerAssetCut;
        var attackTowerAsset = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        if(attackTowerAsset!=null)
        {
            towerImage.sprite = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        }

        var projectileId = towerData.Projectile_ID;
        var projectileData = DataTableManager.ProjectileTable.Get(projectileId);
        var projectileSpecialEffectId = projectileData.ProjectileProperties1_ID;

        int startIndex = 0;

        if (projectileSpecialEffectId != 0)
        {
            var projectileAbilityData = GetRandomAbilityDataFromTowerId(towerId);
            var abilityName = projectileAbilityData.RandomAbilityName;
            var abilityValue = projectileData.ProjectileProperties1Value.ToString();
            var specialData = DataTableManager.SpecialEffectTable.Get(projectileSpecialEffectId);
            
            var firstAbilityPanel = abilityPanels[0];

            selfAbilityPanels[0].SetActive(true);
            firstAbilityPanel.SetActive(true);
            var selfAbilityTexts = firstAbilityPanel.GetComponentsInChildren<TextMeshProUGUI>();
            if (selfAbilityTexts.Length != 2)
                return;
            
            var selfAbilityImage = firstAbilityPanel.GetComponentInChildren<Image>();
            if (selfAbilityImage == null)
                return;
            
            selfAbilityImage.sprite = LoadManager.GetLoadedGameTexture(specialData.SpecialEffectIcon);
            selfAbilityTexts[0].text = abilityName;
            selfAbilityTexts[1].text = abilityValue;
            startIndex = 1;
        }
        
        var abilityData = DataTableManager.RandomAbilityTable.Get(ability);
        var specialEffectName = abilityData.RandomAbilityName;
        var specialEffectValue = abilityData.SpecialEffectValue;
        var randomAbilityObj1 = abilityPanels[startIndex];
        
        randomAbilityObj1.SetActive(true);
        var abilityTexts = randomAbilityObj1.GetComponentsInChildren<TextMeshProUGUI>();
        
        if (abilityTexts.Length != 2)
            return;

        var selfAbilityImage1 = randomAbilityObj1.GetComponentInChildren<Image>();
        if (selfAbilityImage1 == null)
            return;

        // selfAbilityImage1.sprite = LoadManager.GetLoadedGameTexture(abilityData.RandomAbilityIcon);
        var abilityId1 = abilityData.SpecialEffect_ID;
        var abilityData1 = DataTableManager.SpecialEffectTable.Get(abilityId1);
        var abilityIcon1 = abilityData1.SpecialEffectIcon;
        selfAbilityImage1.sprite = LoadManager.GetLoadedGameTexture(abilityIcon1);

        abilityTexts[0].text = specialEffectName;
        abilityTexts[1].text = abilityData1.SpecialEffectValueType == 1 ? $"{specialEffectValue}%" : $"{specialEffectValue}";
        startIndex++;

        if (!abilityData.SpecialEffect2_ID.HasValue || abilityData.SpecialEffect2_ID.Value == 0) return;

        var specialEffectName2 = abilityData.RandomAbility2Name;
        var specialEffectValue2 = abilityData.SpecialEffect2Value;
        var randomAbilityObj2 = abilityPanels[startIndex];

        randomAbilityObj2.SetActive(true);
        var abilityTexts2 = randomAbilityObj2.GetComponentsInChildren<TextMeshProUGUI>();
        if (abilityTexts2.Length != 2) return;

        var selfAbilityImage2 = randomAbilityObj1.GetComponentInChildren<Image>();
        if (selfAbilityImage2 == null)
            return;

        // selfAbilityImage2.sprite = LoadManager.GetLoadedGameTexture(abilityData.RandomAbilityIcon);
        var abilityId2 = abilityData.SpecialEffect2_ID.Value;
        var abilityData2 = DataTableManager.SpecialEffectTable.Get(abilityId2);
        var abilityIcon2 = abilityData2.SpecialEffectIcon;
        selfAbilityImage2.sprite = LoadManager.GetLoadedGameTexture(abilityIcon2);

        abilityTexts2[0].text = specialEffectName2;
        abilityTexts2[1].text = abilityData2.SpecialEffectValueType == 1 ? $"{specialEffectValue2}%" : $"{specialEffectValue2}";
        startIndex++;

        if (!abilityData.SpecialEffect3_ID.HasValue || abilityData.SpecialEffect3_ID.Value == 0) return;

        var specialEffectName3 = abilityData.RandomAbility3Name;
        var specialEffectValue3 = abilityData.SpecialEffect3Value;
        var randomAbilityObj3 = abilityPanels[startIndex];

        randomAbilityObj3.SetActive(true);
        var abilityTexts3 = randomAbilityObj3.GetComponentsInChildren<TextMeshProUGUI>();
        if (abilityTexts3.Length != 2)
            return;

        var selfAbilityImage3 = randomAbilityObj1.GetComponentInChildren<Image>();
        if (selfAbilityImage3 == null)
            return;

        // selfAbilityImage3.sprite = LoadManager.GetLoadedGameTexture(abilityData.RandomAbilityIcon);
        var abilityId3 = abilityData.SpecialEffect3_ID.Value;
        var abilityData3 = DataTableManager.SpecialEffectTable.Get(abilityId3);
        var abilityIcon3 = abilityData3.SpecialEffectIcon;
        selfAbilityImage3.sprite = LoadManager.GetLoadedGameTexture(abilityIcon3);

        abilityTexts3[0].text = specialEffectName3;
        abilityTexts3[1].text = abilityData3.SpecialEffectValueType == 1 ? $"{specialEffectValue3}%" : $"{specialEffectValue3}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetTowerName(string towerName)
    {
        towerNameText.text = towerName;
    }

    private void SetTowerExplain(string explain)
    {
        towerExplainText.text = explain;
    }

    private void SetSpecialAbility(string abilityName, string abilityValue)
    {
        specialAbilityText.text = $"{abilityName} +{abilityValue}";
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