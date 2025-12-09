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

    private Dictionary<RewardData, int> rewardResults = new Dictionary<RewardData, int>();

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
            OnGacha();

            foreach(var reward in rewardResults)
            {
                Debug.Log($"획득 보상: {reward.Key.Target_Id} x{reward.Value}");
            }
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
                isEnough = UserData.Gold >= totalCost;
                if(isEnough)
                {
                    UserData.Gold -= totalCost;
                }
                break;
            case CurrencyType.FreeDia:
                isEnough = UserData.FreeDia >= totalCost;
                if(isEnough)
                {
                    UserData.FreeDia -= totalCost;
                }
                break;
            case CurrencyType.FreePlusChargedDia:
                isEnough = (UserData.FreeDia + UserData.ChargedDia) >= totalCost;
                if(isEnough)
                {
                    while(totalCost > 0 && UserData.FreeDia > 0)
                    {
                        int minus = Mathf.Min(UserData.FreeDia, totalCost);
                        UserData.FreeDia -= minus;
                        totalCost -= minus;
                    }

                    while(totalCost > 0 && UserData.ChargedDia > 0)
                    {
                        int minus = Mathf.Min(UserData.ChargedDia, totalCost);
                        UserData.ChargedDia -= minus;
                        totalCost -= minus;
                    }
                }
                break;
            case CurrencyType.ChargedDia:
                isEnough = UserData.ChargedDia >= totalCost;
                if(isEnough)
                {
                    UserData.ChargedDia -= totalCost;
                }
                break;
        }

        return isEnough;
    }

    private void OnGacha()
    {
        var draws = DataTableManager.DrawTable.GetRandomDrawData(drawGroup, drawCount);

        foreach(var draw in draws)
        {
            var reward = DataTableManager.RewardTable.Get(draw.Reward_Id);
            
            UserDataMapper.AddItem(reward.Target_Id, draw.RewardQty);

            if(rewardResults.ContainsKey(reward))
            {
                if(reward.Stack == 1)
                {
                    var addCount = Mathf.Min(rewardResults[reward] + draw.RewardQty, UserDataMapper.GetMaxCount(reward.Target_Id));
                    UserDataMapper.AddItem(reward.Target_Id, draw.RewardQty);
                    rewardResults[reward] = addCount;
                }
                else if(reward.RewardType == (int)RewardType.Planet)
                {
                    var pieceId = PlanetPieceMapper.GetPieceId(reward.Target_Id);
                    var addCount = Mathf.Min(rewardResults[reward] + 20, UserDataMapper.GetMaxCount(reward.Target_Id));
                    UserDataMapper.AddItem(pieceId, 20);
                    rewardResults[reward] = addCount;
                }
            }
            else
            {
                rewardResults.Add(reward, draw.RewardQty);
            }

            Debug.Log($"획득 보상: {draw.Draw_Id} / {reward.RewardNameText}");
        }
    }
}
