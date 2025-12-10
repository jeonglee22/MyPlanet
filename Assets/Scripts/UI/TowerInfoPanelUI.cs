using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoPanelUI : MonoBehaviour
{
    [SerializeField] private Button exitBtn;

    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerDescriptionText;

    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI attackRangeText;
    [SerializeField] private TextMeshProUGUI accuracyRateText;
    [SerializeField] private TextMeshProUGUI concentrationModulusText;
    [SerializeField] private TextMeshProUGUI projectileCountText;

    private void Start()
    {
        exitBtn.onClick.AddListener(OnExitBtnClicked);
    }

    private void OnDestroy()
    {
        exitBtn.onClick.RemoveListener(OnExitBtnClicked);
    }

    public void Initialize(AttackTowerTableRow data)
    {
        var towerDescData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = data.AttackTowerName.Split("\\n")[0];
        towerDescriptionText.text = towerDescData.TowerDescribe;

        attackSpeedText.text = $"{data.AttackSpeed}";
        attackRangeText.text = $"{data.AttackRange}";
        accuracyRateText.text = $"{data.Accuracy}";
        concentrationModulusText.text = $"{data.grouping}";
        projectileCountText.text = $"{data.ProjectileNum}";
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
    }
    
}
