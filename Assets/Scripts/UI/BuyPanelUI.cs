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
        buyConfirmUI.gameObject.SetActive(false);
        noCurrencyText.gameObject.SetActive(false);
    }

    private bool TryPay()
    {
        bool isEnough = false;
        switch ((CurrencyType)drawGroup)
        {
            case CurrencyType.Gold:
                isEnough = Variables.Gold >= needCurrencyValue * drawCount;
                break;
            case CurrencyType.FreeDia:
                isEnough = Variables.FreeDia >= needCurrencyValue * drawCount;
                break;
            case CurrencyType.FreePlusChargedDia:
                isEnough = (Variables.FreeDia + Variables.ChargedDia) >= needCurrencyValue * drawCount;
                break;
            case CurrencyType.ChargedDia:
                isEnough = Variables.ChargedDia >= needCurrencyValue * drawCount;
                break;
        }

        return isEnough;
    }

    private void OnGacha()
    {
        
    }
}
