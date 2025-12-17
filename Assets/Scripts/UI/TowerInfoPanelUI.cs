using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoPanelUI : MonoBehaviour
{
    [SerializeField] private Button exitBtn;

    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerDescriptionText;

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
    [SerializeField] private TextMeshProUGUI extraNumberText;

    private void Awake()
    {
        exitBtn.onClick.AddListener(OnExitBtnClicked);
    }

    private void OnDestroy()
    {
        exitBtn.onClick.RemoveListener(OnExitBtnClicked);
    }

    private void OnDisable()
    {
        extraNumberObj.SetActive(false);
    }

    public void Initialize(AttackTowerTableRow data)
    {
        var towerDescData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);
        var projectileData = DataTableManager.ProjectileTable.Get(data.Projectile_ID);
        SpecialEffectData specialEffectData = null;

        var towerUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = towerUpgradeData.towerIds.IndexOf(data.AttackTower_Id);
        var towerLevel = towerUpgradeData.upgradeLevels[towerIndex];

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
            specialEffectData = DataTableManager.SpecialEffectTable.Get(projectileData.ProjectileProperties1_ID);
            extraNumberObj.SetActive(true);
            extraNumberText.text = $"{projectileData.ProjectileProperties1Value}";
        }

        towerNameText.text = towerDescData.TowerName;
        towerDescriptionText.text = towerDescData.TowerDescribe;

        SetExplainText(attackText, projectileData.Attack, additionalAttack);
        SetExplainText(attackSpeedText, data.AttackSpeed, additionalAttackSpeed);
        SetExplainText(ratioPenetrationText, projectileData.RatePenetration, 0f, "%");
        SetExplainText(fixedPenetrationText, projectileData.FixedPenetration, 0f);
        SetExplainText(accuracyRateText, data.Accuracy, 0f, "%");
        SetExplainText(projectileCountText, data.ProjectileNum, additionalProjectileNum);
        SetExplainText(targetCountText, projectileData.TargetNum, 0f);
        SetExplainText(concentrationModulusText, data.grouping, 0f, "%");
        SetExplainText(MaintenanceTimeText, projectileData.RemainTime, additionalDuration, "초");

        // attackText.text = $"{projectileData.Attack}";
        // attackSpeedText.text = $"{data.AttackSpeed}";
        // ratioPenetrationText.text = $"{projectileData.RatePenetration}%";
        // fixedPenetrationText.text = $"{projectileData.FixedPenetration}";
        // accuracyRateText.text = $"{data.Accuracy}%";
        // projectileCountText.text = $"{data.ProjectileNum}";
        // targetCountText.text = $"{projectileData.TargetNum}";
        // concentrationModulusText.text = $"{data.grouping}%";
        // MaintenanceTimeText.text = $"{projectileData.RemainTime}초";
    }

    private void SetExplainText(TextMeshProUGUI textComponent, float baseValue, float additionalValue, string suffix = "")
    {
        if (additionalValue > 0)
        {
            textComponent.text = $"{baseValue} (+{additionalValue}){suffix}";
        }
        else
        {
            textComponent.text = $"{baseValue}{suffix}";
        }
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
    }
    
}
