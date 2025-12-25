using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoPanelUI : MonoBehaviour
{
    [SerializeField] private Button exitBtn;

    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerDescriptionText;

    [SerializeField] private GameObject totalTowerPanel;

    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI ratioPenetrationText;
    [SerializeField] private TextMeshProUGUI fixedPenetrationText;
    [SerializeField] private TextMeshProUGUI accuracyRateText;
    [SerializeField] private TextMeshProUGUI projectileCountText;
    [SerializeField] private TextMeshProUGUI targetCountText;
    [SerializeField] private TextMeshProUGUI concentrationModulusText;
    [SerializeField] private TextMeshProUGUI MaintenanceTimeText;
    [SerializeField] private GameObject extraNumberObj;
    [SerializeField] private Image extraNumberImg;
    [SerializeField] private TextMeshProUGUI extraNameText;
    [SerializeField] private TextMeshProUGUI extraNumberText;

    [SerializeField] private List<GameObject> upgradeObjects;
    [SerializeField] private List<GameObject> blinkingObjects;
    // [SerializeField] private GameObject secondLevelUpgradeObj;
    // [SerializeField] private GameObject thirdLevelUpgradeObj;
    // [SerializeField] private GameObject lastLevelUpgradeObj;

    private float attackSum = 0;
    private float attackSpeedSum = 0;
    private float projectileCountSum = 0;
    private float durationSum = 0;
    private float explosionRangeSum = 0;

    private float blinkingInterval = 0.5f;
    private float blinkingTimer = 0f;
    private bool isOff = false;
    private int currentLevel = 0;

    private void Awake()
    {
        exitBtn.onClick.AddListener(OnExitBtnClicked);
        exitBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
    }

    private void OnDestroy()
    {
        exitBtn.onClick.RemoveAllListeners();
    }

    private void OnDisable()
    {
        // extraNumberObj?.SetActive(false);
    }

    private void Update()
    {
        if (currentLevel >= 4)
            return;

        if (blinkingObjects == null || blinkingObjects.Count == 0)
            return;
        if (upgradeObjects == null || upgradeObjects.Count == 0)
            return;
        
        blinkingTimer += Time.unscaledDeltaTime;
        if (blinkingTimer >= blinkingInterval)
        {
            blinkingTimer = 0f;
            if (isOff)
            {
                SetDisableUpgradeObject(blinkingObjects[currentLevel]);
                isOff = false;
            }
            else
            {
                SetEnableUpgradeObject(blinkingObjects[currentLevel]);
                isOff = true;
            }
        }
    }

    private void ResetValue()
    {
        attackSum = 0;
        attackSpeedSum = 0;
        projectileCountSum = 0;
        durationSum = 0;
        explosionRangeSum = 0;
    }

    public void InitializeInfo(AttackTowerTableRow data)
    {
        var towerDescData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);
        var projectileData = DataTableManager.ProjectileTable.Get(data.Projectile_ID);

        var towerUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = towerUpgradeData.towerIds.IndexOf(data.AttackTower_Id);
        var towerLevel = towerUpgradeData.upgradeLevels[towerIndex];

        iconImg.sprite = LoadManager.GetLoadedGameTexture(data.AttackTowerAsset);

        var additionalAttack = 0f;
        var additionalAttackSpeed = 0f;
        var additionalDuration = 0f;
        var additionalProjectileNum = 0;
        var additionalExplosionRange = 0f;

        var externalTowerUpgradeDataList = new List<TowerUpgradeData>();
        for (int i = 1; i <= towerLevel; i++)
        {
            var externalTowerUpgradeDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(data.AttackTower_Id, i);
            if (externalTowerUpgradeDataId != -1)
            {
                var externalTowerUpgradeData = DataTableManager.TowerUpgradeTable.Get(externalTowerUpgradeDataId);
                externalTowerUpgradeDataList.Add(externalTowerUpgradeData);
            }
        }

        towerLevel = Mathf.Min(towerLevel, 3);
        for (int i = 1; i <= towerLevel; i++)
        {
            var specialEffectId = externalTowerUpgradeDataList[i - 1].SpecialEffect_ID;
            var amount = externalTowerUpgradeDataList[i - 1].SpecialEffectValue;
            switch (specialEffectId)
            {
                case (int)SpecialEffectId.AttackSpeed:
                    additionalAttackSpeed += amount;
                    break;
                case (int)SpecialEffectId.Attack:
                    additionalAttack += amount;
                    break;
                case (int)SpecialEffectId.Duration:
                    additionalDuration += amount;
                    break;
                case (int)SpecialEffectId.ProjectileCount:
                    additionalProjectileNum += (int)amount;
                    break;
                case (int)SpecialEffectId.Explosion:
                    additionalExplosionRange += amount;
                    break;
                default:
                    break;   
            }
        }

        if(projectileData.ProjectileProperties1_ID == 0)
        {
            extraNumberObj.SetActive(false);
        }
        else
        {
            extraNumberObj.SetActive(true);
            var effectData = DataTableManager.SpecialEffectTable.Get(projectileData.ProjectileProperties1_ID);
            var effectTextDataId = effectData.SpecialEffectText_ID;
            var effectTextData = DataTableManager.SpecialEffectTextTable.Get(effectTextDataId);
            var effectName = effectTextData.Name;
            var effectIcon = LoadManager.GetLoadedGameTexture(effectData.SpecialEffectIcon);
            
            extraNumberImg.sprite = effectIcon;
            extraNameText.text = effectName;
            extraNumberText.text = $"{projectileData.ProjectileProperties1Value}";
        }

        towerNameText.text = towerDescData.TowerName;
        towerDescriptionText.text = towerDescData.TowerDescribe;

        SetExplainText(attackText, projectileData.Attack, additionalAttack,"%");
        SetExplainText(attackSpeedText, data.AttackSpeed, additionalAttackSpeed, "%");
        SetExplainText(ratioPenetrationText, projectileData.RatePenetration, 0f, "","%");
        SetExplainText(fixedPenetrationText, projectileData.FixedPenetration, 0f);
        SetExplainText(accuracyRateText, data.Accuracy, 0f, "", "%");
        SetExplainText(projectileCountText, data.ProjectileNum, additionalProjectileNum);
        SetExplainText(targetCountText, projectileData.TargetNum, 0f);
        SetExplainText(concentrationModulusText, data.grouping, 0f, "", "%");
        SetExplainText(MaintenanceTimeText, projectileData.RemainTime, additionalDuration, "", "초");
    }

    public void InitializeUpgrade(AttackTowerTableRow data)
    {
        ResetValue();

        var attackTowerId = data.AttackTower_Id;

        var towerDescData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);
        var projectileData = DataTableManager.ProjectileTable.Get(data.Projectile_ID);
        // SpecialEffectData specialEffectData = null;

        var towerUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = towerUpgradeData.towerIds.IndexOf(attackTowerId);
        var towerLevel = towerUpgradeData.upgradeLevels[towerIndex];

        iconImg.sprite = LoadManager.GetLoadedGameTexture(data.AttackTowerAsset);

        currentLevel = towerLevel;

        for (int i = 0; i < blinkingObjects.Count; i++)
        {
            if (i < towerLevel)
            {
                SetEnableUpgradeObject(blinkingObjects[i]);
            }
            else
            {
                SetDisableUpgradeObject(blinkingObjects[i]);
            }
        }

        towerNameText.text = towerDescData.TowerName;
        towerDescriptionText.text = towerDescData.TowerDescribe;

        var firstLevelDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(attackTowerId, 1);
        var secondLevelDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(attackTowerId, 2);
        var thirdLevelDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(attackTowerId, 3);

        var firstSpecialEffectId = DataTableManager.TowerUpgradeTable.Get(firstLevelDataId).SpecialEffect_ID;
        var firstSpecialEffectValue = DataTableManager.TowerUpgradeTable.Get(firstLevelDataId).SpecialEffectValue;
        var secondSpecialEffectId = DataTableManager.TowerUpgradeTable.Get(secondLevelDataId).SpecialEffect_ID;
        var secondSpecialEffectValue = DataTableManager.TowerUpgradeTable.Get(secondLevelDataId).SpecialEffectValue;
        var thirdSpecialEffectId = DataTableManager.TowerUpgradeTable.Get(thirdLevelDataId).SpecialEffect_ID;
        var thirdSpecialEffectValue = DataTableManager.TowerUpgradeTable.Get(thirdLevelDataId).SpecialEffectValue;

        AddValueOfEffectId(firstSpecialEffectId, firstSpecialEffectValue, 0);
        AddValueOfEffectId(secondSpecialEffectId, secondSpecialEffectValue, 1);
        AddValueOfEffectId(thirdSpecialEffectId, thirdSpecialEffectValue, 2);

        var upgradeObj = upgradeObjects[3];
        var outTowerUpgradeAbiltyUnlock = upgradeObj.GetComponent<OutTowerUpgradeEachLevelUI>();

        var lastEffectIcons = new List<Sprite>();
        var lastEffectIds = new List<int>();
        var lastEffectValues = new List<float>();

        var lastLevelDataId = DataTableManager.TowerUpgradeAbilityUnlockTable.GetDataId(attackTowerId);
        var lastLevelData = DataTableManager.TowerUpgradeAbilityUnlockTable.Get(lastLevelDataId);
        var lastLevelRandomAbilityId = lastLevelData.RandomAbility_ID;
        var lastLevelAbilityData = DataTableManager.RandomAbilityTable.Get(lastLevelRandomAbilityId);

        var lastFirstEffectId = lastLevelAbilityData.SpecialEffect_ID;
        var lastFirstEffectValue = lastLevelAbilityData.SpecialEffectValue;
        var lastFirstEffectIcon = DataTableManager.SpecialEffectTable.Get(lastFirstEffectId).SpecialEffectIcon;

        lastEffectIcons.Add(LoadManager.GetLoadedGameTexture(lastFirstEffectIcon));
        lastEffectIds.Add(lastFirstEffectId);
        lastEffectValues.Add(lastFirstEffectValue);

        var lastSecondEffectId = lastLevelAbilityData.SpecialEffect2_ID;
        if (lastSecondEffectId != null && lastSecondEffectId.Value != 0)
        {
            var lastSecondEffectValue = lastLevelAbilityData.SpecialEffect2Value;
            var lastSecondEffectIcon = DataTableManager.SpecialEffectTable.Get(lastSecondEffectId.Value).SpecialEffectIcon;

            lastEffectIcons.Add(LoadManager.GetLoadedGameTexture(lastSecondEffectIcon));
            lastEffectIds.Add(lastSecondEffectId.Value);
            lastEffectValues.Add(lastSecondEffectValue.Value);
        }

        var lastThirdEffectId = lastLevelAbilityData.SpecialEffect3_ID;
        if (lastThirdEffectId != null && lastThirdEffectId.Value != 0)
        {
            var lastThirdEffectValue = lastLevelAbilityData.SpecialEffect3Value;
            var lastThirdEffectIcon = DataTableManager.SpecialEffectTable.Get(lastThirdEffectId.Value).SpecialEffectIcon;

            lastEffectIcons.Add(LoadManager.GetLoadedGameTexture(lastThirdEffectIcon));
            lastEffectIds.Add(lastThirdEffectId.Value);
            lastEffectValues.Add(lastThirdEffectValue.Value);
        }

        outTowerUpgradeAbiltyUnlock.SettingUpgradeUiInfoAll(lastEffectIcons, lastEffectIds, lastEffectValues);
    }

    private void AddValueOfEffectId(int effectId, float value, int index)
    {
        var baseValue = 0f;
        var nextValue = 0f;
        bool isPercentValue = false;

        switch (effectId)
        {
            case (int)SpecialEffectId.Attack:
                baseValue = 100 + attackSum;
                attackSum += value;
                nextValue = 100 + attackSum;
                isPercentValue = true;
                break;
            case (int)SpecialEffectId.AttackSpeed:
                baseValue = 100 + attackSpeedSum;
                attackSpeedSum += value;
                nextValue = 100 + attackSpeedSum;
                isPercentValue = true;
                break;
            case (int)SpecialEffectId.Duration:
                baseValue = durationSum;
                durationSum += value;
                nextValue = durationSum;
                break;
            case (int)SpecialEffectId.ProjectileCount:
                baseValue = projectileCountSum;
                projectileCountSum += value;
                nextValue = projectileCountSum;
                break;
            case (int)SpecialEffectId.Explosion:
                baseValue = 100 + explosionRangeSum;
                explosionRangeSum += value;
                nextValue = 100 + explosionRangeSum;
                isPercentValue = true;
                break;
            default:
                break;
        }

        var upgradeObj = upgradeObjects[index];
        var outTowerUpgrade = upgradeObj.GetComponent<OutTowerUpgradeEachLevelUI>();

        var specialEffectData = DataTableManager.SpecialEffectTable.Get(effectId);
        var specialEffectTextId = specialEffectData.SpecialEffectText_ID;
        var specialEffectTextData = DataTableManager.SpecialEffectTextTable.Get(specialEffectTextId);
        var effectName = specialEffectTextData.Name;
        var effectIcon = LoadManager.GetLoadedGameTexture(specialEffectData.SpecialEffectIcon);

        if (isPercentValue)
        {
            outTowerUpgrade.SettingUpgradeUiInfo(effectIcon, effectName, $"{baseValue}%", $"{nextValue}%");
        }
        else
        {
            outTowerUpgrade.SettingUpgradeUiInfo(effectIcon, effectName, $"{baseValue}", $"{nextValue}");
        }
    }

    private void SetExplainText(TextMeshProUGUI textComponent, float baseValue, float additionalValue, string additionalSuffix = "", string suffix = "")
    {
        if (additionalValue > 0)
        {
            textComponent.text = $"{baseValue}\n(+{additionalValue}{additionalSuffix}){suffix}";
        }
        else
        {
            textComponent.text = $"{baseValue}{suffix}";
        }
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
        if (totalTowerPanel != null)
            totalTowerPanel?.SetActive(true);
    }
    
    private void SetDisableUpgradeObject(GameObject go)
    {
        var images = go.GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            img.color = Color.gray;
        }
        var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            text.color = Color.gray;
        }
    }

    private void SetEnableUpgradeObject(GameObject go)
    {
        var images = go.GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            img.color = Color.white;
        }
        var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            text.color = Color.white;
        }
    }
}
