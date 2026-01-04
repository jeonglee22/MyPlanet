using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeToweCardUiSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerImage;

    [SerializeField] private Image[] upgradeStarImages;
    [SerializeField] private List<GameObject> upgradeAbilityObjects;
    [SerializeField] private List<Image> upgradeAbilityIcons;
    [SerializeField] private List<TextMeshProUGUI> upgradeAbilityNameTexts;
    [SerializeField] private List<TextMeshProUGUI> upgradeAbilityValueTexts;

    private float blinkingTime = 0.5f;
    private float currentTime = 0f;
    private bool isStarVisible = true;

    private int nextLevel = 0;

    public void SettingAttackTowerUpgradeCard(int towerId, int towerLevel)
    {
        upgradeAbilityObjects[1].SetActive(false);
        upgradeAbilityObjects[2].SetActive(false);

        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var towerExplainId = towerData.TowerText_ID;
        var towerExplainData = DataTableManager.TowerExplainTable.Get(towerExplainId);
        var towerName = towerExplainData.TowerName;

        var attackRow = DataTableManager.AttackTowerTable.GetById(towerId);
        float addValue = TowerReinforceManager.Instance.GetAttackAddValueByIds(
                attackRow.TowerReinforceUpgrade_ID,
                towerLevel
            );

        var abilityIcon = LoadManager.GetLoadedGameTexture("Att_icon");
        upgradeAbilityIcons[0].sprite = abilityIcon;
        upgradeAbilityNameTexts[0].text = "공격력";
        upgradeAbilityValueTexts[0].text = addValue % 1 == 0 ? $"+ {addValue:F0}" : $"+ {addValue:F1}";

        SetTowerName(towerName);

        var attackTowerAssetName = towerData.AttackTowerAssetCut;
        var attackTowerAsset = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        if(attackTowerAsset!=null)
        {
            towerImage.sprite = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        }
        Debug.Log(towerName + "Upgrade Tower Level: " + towerLevel);
        for(int i=0; i<upgradeStarImages.Length; i++)
        {
            if(i<towerLevel)
            {
                upgradeStarImages[i].gameObject.SetActive(true);
            }
            else
            {
                upgradeStarImages[i].gameObject.SetActive(false);
            }
        }

        nextLevel = towerLevel;
    }

    public void SettingAmplifierTowerUpgradeCard(int towerId, int towerLevel)
    {
        var towerData = DataTableManager.BuffTowerTable.Get(towerId);
        var towerNameId = towerData.TowerText_ID;
        var towerNameData = DataTableManager.TowerExplainTable.Get(towerNameId);
        var towerName = towerNameData.TowerName;

        SetTowerName(towerName);

        int[] reinforceIds = towerData.BuffTowerReinforceUpgrade_ID;

        SetAmplifierReinforceUpgradeCard(reinforceIds, towerLevel);

        var amplifierTowerAssetName = towerData.BuffTowerAssetCut;
        var amplifierTowerAsset = LoadManager.GetLoadedGameTexture(amplifierTowerAssetName);
        if(amplifierTowerAsset!=null)
        {
            towerImage.sprite = LoadManager.GetLoadedGameTexture(amplifierTowerAssetName);
        }
        Debug.Log(towerName + "Upgrade Tower Level: " + towerLevel);
        for(int i=0; i<upgradeStarImages.Length; i++)
        {
            if(i<towerLevel)
            {
                upgradeStarImages[i].gameObject.SetActive(true);
            }
            else
            {
                upgradeStarImages[i].gameObject.SetActive(false);
            }
        }

        nextLevel = towerLevel;
    }

    private void SetAmplifierReinforceUpgradeCard(int[] reinforceIds, int towerLevel)
    {
        for(int i = 0; i < upgradeAbilityObjects.Count; i++)
        {
            upgradeAbilityObjects[i].SetActive(false);
        }

        if (reinforceIds == null || reinforceIds.Length == 0) return;

        var extraEffects = TowerReinforceManager.GetBuffAddValuesByIdsStatic(reinforceIds, towerLevel);
    
        if (extraEffects == null || extraEffects.Count == 0) return;

        int displayIndex = 0;

        foreach(var effectKV in extraEffects)
        {
            if(displayIndex >= upgradeAbilityObjects.Count) break;
            
            int specialEffectId = effectKV.Key;
            float addValue = effectKV.Value;
            
            if(specialEffectId == 0) continue;
            
            var specialEffectData = DataTableManager.SpecialEffectTable.Get(specialEffectId);
            if(specialEffectData == null) continue;
            
            var abilityIcon = LoadManager.GetLoadedGameTexture(specialEffectData.SpecialEffectIcon);
            
            upgradeAbilityObjects[displayIndex].SetActive(true);
            upgradeAbilityIcons[displayIndex].sprite = abilityIcon;
            upgradeAbilityNameTexts[displayIndex].text = specialEffectData.SpecialEffectName;
            upgradeAbilityValueTexts[displayIndex].text = addValue % 1 == 0 ? $"+ {addValue:F0}" : $"+ {addValue:F1}";
            
            displayIndex++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.unscaledDeltaTime;
        if(currentTime>=blinkingTime)
        {
            isStarVisible = !isStarVisible;
            currentTime = 0f;
            upgradeStarImages[nextLevel - 1].gameObject.SetActive(isStarVisible);
        }
    }

    private void SetTowerName(string towerName)
    {
        towerNameText.text = towerName;
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
