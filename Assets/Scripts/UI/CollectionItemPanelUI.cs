using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectionItemPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI rateText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private Button weightUpBtn;
    [SerializeField] private Button weightDownBtn;
    [SerializeField] private Button infoBtn;

    private bool isTower = false;
    private bool isAttackTower = false;
    private bool isBuffTower = false;

    public bool IsTower => isTower;
    public bool IsAttackTower => isAttackTower;
    public bool IsBuffTower => isBuffTower;

    private int weightId;

    public event Action OnWeightChanged;
    public event Action<CollectionItemPanelUI> OnInfoBtn;

    public AttackTowerTableRow AttackTowerData { get; private set; }
    public BuffTowerData BuffTowerData { get; private set; }

    private void OnDestroy()
    {
        weightUpBtn.onClick.RemoveListener(OnWeightUpBtn);
        weightDownBtn.onClick.RemoveListener(OnWeightDownBtn);
        infoBtn.onClick.RemoveListener(OnInfoBtnClicked);
    }

    public void Initialize(AttackTowerTableRow data, int dataCount, bool isTower)
    {
        var textData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = textData.TowerName;

        weightId = data.AttackTower_Id;
        this.isTower = isTower;
        isAttackTower = true;

        AttackTowerData = data;

        UpdateWeightDisplay();

        weightUpBtn.onClick.AddListener(OnWeightUpBtn);
        weightDownBtn.onClick.AddListener(OnWeightDownBtn);
        infoBtn.onClick.AddListener(OnInfoBtnClicked);
    }

    public void Initialize(BuffTowerData data, int dataCount, bool isTower)
    {
        var textData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = textData.TowerName;

        weightId = data.BuffTower_ID;
        this.isTower = isTower;
        isBuffTower = true;

        BuffTowerData = data;

        UpdateWeightDisplay();

        weightUpBtn.onClick.AddListener(OnWeightUpBtn);
        weightDownBtn.onClick.AddListener(OnWeightDownBtn);
        infoBtn.onClick.AddListener(OnInfoBtnClicked);
    }

    public void UpdateWeightDisplay()
    {
        float currentWeight = CollectionManager.Instance.GetWeight(weightId);

        float totalWeight = 0f;

        if(isAttackTower)
        {
            totalWeight = CollectionManager.Instance.GetTotalWeightAttackTowers();
        }
        else if(isBuffTower)
        {
            totalWeight = CollectionManager.Instance.GetTotalWeightBuffTowers();
        }

        float probability = totalWeight > 0 ? (currentWeight / totalWeight) * 100f : 0f;
        
        rateText.text = $"확률: {probability:F2}%";
        weightText.text = $"가중치: {currentWeight}";
    }

    public void OnWeightUpBtn()
    {
        if(CollectionManager.Instance.TryIncreaseWeight(weightId, isTower))
        {
            OnWeightChanged?.Invoke();
        }
    }

    public void OnWeightDownBtn()
    {
        if(CollectionManager.Instance.TryDecreaseWeight(weightId, isTower))
        {
            OnWeightChanged?.Invoke();
        }
    }

    public void OnInfoBtnClicked()
    {
        OnInfoBtn?.Invoke(this);
    }
}
