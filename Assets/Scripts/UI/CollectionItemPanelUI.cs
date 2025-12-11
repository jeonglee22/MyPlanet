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

    private int upCount = 0;

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

        float weight = data.TowerWeight;
        float calWeight = weight / dataCount * 100f;
        rateText.text = $"확률: {calWeight:F2}%";
        weightText.text = $"가중치: {weight}";

        weightUpBtn.onClick.AddListener(OnWeightUpBtn);
        weightDownBtn.onClick.AddListener(OnWeightDownBtn);
        infoBtn.onClick.AddListener(OnInfoBtnClicked);

        this.isTower = isTower;
        isAttackTower = true;

        AttackTowerData = data;
    }

    public void Initialize(BuffTowerData data, int dataCount, bool isTower)
    {
        var textData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = textData.TowerName;

        float weight = data.TowerWeight;
        float calWeight = weight / dataCount * 100f;
        rateText.text = $"확률: {calWeight:F2}%";
        weightText.text = $"가중치: {weight}";

        weightUpBtn.onClick.AddListener(OnWeightUpBtn);
        weightDownBtn.onClick.AddListener(OnWeightDownBtn);
        infoBtn.onClick.AddListener(OnInfoBtnClicked);

        this.isTower = isTower;
        isBuffTower = true;

        BuffTowerData = data;
    }

    public void OnWeightUpBtn()
    {
        if(!TryPay())
        {
            Debug.Log("Not Enough Core");
            return;
        }
        OnWeightChanged?.Invoke();
    }

    public void OnWeightDownBtn()
    {
        CorePayUp();
        OnWeightChanged?.Invoke();
    }

    private bool TryPay()
    {
        if (isTower && UserData.CollectionTowerCore > 0)
        {
            UserData.CollectionTowerCore -= 1;
            upCount++;
            return true;
        }
        else if (!isTower && UserData.CollectionRandomAbilityCore > 0)
        {
            UserData.CollectionRandomAbilityCore -= 1;
            upCount++;
            return true;
        }

        return false;
    }

    private void CorePayUp()
    {
        if(upCount <= 0)
        {
            return;
        }

        if(isTower)
        {
            UserData.CollectionTowerCore += 1;
        }
        else
        {
            UserData.CollectionRandomAbilityCore += 1;
        }

        upCount--;
    }

    public void OnInfoBtnClicked()
    {
        OnInfoBtn?.Invoke(this);
    }
}
