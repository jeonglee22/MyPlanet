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

        attackText.text = $"{projectileData.Attack}";
        attackSpeedText.text = $"{data.AttackSpeed}";
        ratioPenetrationText.text = $"{projectileData.RatePenetration}%";
        fixedPenetrationText.text = $"{projectileData.FixedPenetration}";
        accuracyRateText.text = $"{data.Accuracy}%";
        projectileCountText.text = $"{data.ProjectileNum}";
        targetCountText.text = $"{projectileData.TargetNum}";
        concentrationModulusText.text = $"{data.grouping}%";
        MaintenanceTimeText.text = $"{projectileData.RemainTime}초";
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
    }
    
}
