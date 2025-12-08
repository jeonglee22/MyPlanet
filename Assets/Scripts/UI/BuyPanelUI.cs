using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanelUI : MonoBehaviour
{
    private int drawGroup;
    private string gachaName;
    private int drawCount = 0;
    private int needCurrencyValue = 0;

    [SerializeField] private Button backBtn;
    [SerializeField] private TextMeshProUGUI gachaNameText;
    [SerializeField] private Button buyOnceBtn;
    [SerializeField] private Button buyTenBtn;

    [SerializeField] private GameObject buyConfirmUI;
    [SerializeField] private TextMeshProUGUI confirmGachaText;
    [SerializeField] private GameObject noCurrencyText;
    [SerializeField] private Button confirmYesBtn;
    [SerializeField] private Button confirmNoBtn;

    private Dictionary<int, int> rewardStacks = new Dictionary<int, int>();

    public void Initialize(int needCurrencyValue, int drawGroup, string gachaName)
    {
        this.drawGroup = drawGroup;
        this.gachaName = gachaName;
        this.needCurrencyValue = needCurrencyValue;
        drawCount = 0;

        gachaNameText.text = gachaName;

        ResetBtn();

        backBtn.onClick.AddListener(OnBackBtnClicked);
        buyOnceBtn.onClick.AddListener(() => OnGachaClicked(1));
        buyTenBtn.onClick.AddListener(() => OnGachaClicked(10));
        confirmYesBtn.onClick.AddListener(OnConfirmYesBtnClicked);
        confirmNoBtn.onClick.AddListener(OnConfirmNoBtnClicked);
    }

    private void ResetBtn()
    {
        backBtn.onClick.RemoveAllListeners();
        buyOnceBtn.onClick.RemoveAllListeners();
        buyTenBtn.onClick.RemoveAllListeners();
        confirmYesBtn.onClick.RemoveAllListeners();
        confirmNoBtn.onClick.RemoveAllListeners();

        buyConfirmUI.SetActive(false);
    }

    private void OnConfirm(string gachaName, int drawCount)
    {
        confirmGachaText.text = $"{gachaName}를 x{drawCount}회 돌리시겠습니까?";
        noCurrencyText.SetActive(false);
        buyConfirmUI.SetActive(true);
    }

    private void OnBackBtnClicked()
    {
        buyConfirmUI.SetActive(false);
        noCurrencyText.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnGachaClicked(int drawCount)
    {
        this.drawCount = drawCount;

        OnConfirm(gachaName, drawCount);
    }

    private void OnConfirmYesBtnClicked()
    {
        if (TryPay())
        {
            
        }
        else
        {
            noCurrencyText.SetActive(true);
        }
    }

    private void OnConfirmNoBtnClicked()
    {
        buyConfirmUI.SetActive(false);
        noCurrencyText.SetActive(false);
    }

    private bool TryPay()
    {
        bool isEnough = false;
        int totalCost = needCurrencyValue * drawCount;

        switch ((CurrencyType)drawGroup)
        {
            case CurrencyType.Gold:
                isEnough = ItemInventory.Gold >= totalCost;
                if(isEnough)
                {
                    ItemInventory.Gold -= totalCost;
                }
                break;
            case CurrencyType.FreeDia:
                isEnough = ItemInventory.FreeDia >= totalCost;
                if(isEnough)
                {
                    ItemInventory.FreeDia -= totalCost;
                }
                break;
            case CurrencyType.FreePlusChargedDia:
                isEnough = (ItemInventory.FreeDia + ItemInventory.ChargedDia) >= totalCost;
                if(isEnough)
                {
                    while(totalCost > 0 && ItemInventory.FreeDia > 0)
                    {
                        int minus = Mathf.Min(ItemInventory.FreeDia, totalCost);
                        ItemInventory.FreeDia -= minus;
                        totalCost -= minus;
                    }

                    while(totalCost > 0 && ItemInventory.ChargedDia > 0)
                    {
                        int minus = Mathf.Min(ItemInventory.ChargedDia, totalCost);
                        ItemInventory.ChargedDia -= minus;
                        totalCost -= minus;
                    }
                }
                break;
            case CurrencyType.ChargedDia:
                isEnough = ItemInventory.ChargedDia >= totalCost;
                if(isEnough)
                {
                    ItemInventory.ChargedDia -= totalCost;
                }
                break;
        }

        return isEnough;
    }

    private void OnGacha()
    {
        var rewards = DataTableManager.DrawTable.GetRandomDrawData(drawGroup, drawCount);


    }
}
