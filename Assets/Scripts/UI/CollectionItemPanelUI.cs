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

    private bool isTower = false;

    private int upCount = 0;

    public event Action OnWeightChanged;

    public void Initialize(AttackTowerTableRow data, int dataCount, bool isTower)
    {
        towerNameText.text = data.AttackTowerName.Split('\n')[0];

        float weight = data.TowerWeight;
        float calWeight = weight / dataCount * 100f;
        rateText.text = $"확률: {calWeight:F2}%";
        weightText.text = $"가중치: {weight}";

        weightUpBtn.onClick.AddListener(OnWeightUpBtn);
        weightDownBtn.onClick.AddListener(OnWeightDownBtn);

        this.isTower = isTower;
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
}
