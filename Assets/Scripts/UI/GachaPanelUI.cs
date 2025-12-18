using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaPanelUI : MonoBehaviour
{
    private int drawGroup;
    private string gachaName;
    private int drawCount = 0;
    private int needCurrencyValue = 0;

    [SerializeField] private Button backBtn;
    [SerializeField] private TextMeshProUGUI gachaNameText;
    [SerializeField] private Button buyOnceBtn;
    [SerializeField] private Button buyTenBtn;

    [SerializeField] private GameObject gachaConfirmUI;
    [SerializeField] private TextMeshProUGUI confirmGachaText;
    [SerializeField] private GameObject noCurrencyText;
    [SerializeField] private Button confirmYesBtn;
    [SerializeField] private Button confirmNoBtn;

    [SerializeField] private GameObject gachaOncePanelUI;
    [SerializeField] private TextMeshProUGUI rewardNameText;
    [SerializeField] private TextMeshProUGUI rewardOnceText;
    [SerializeField] private Button exitRewardOnceBtn;

    [SerializeField] private GameObject gachaTenPanelUI;
    [SerializeField] private GameObject RewardTenPrefab;
    [SerializeField] private Transform gachaTenContent;
    [SerializeField] private Button exitRewardTenBtn;

    private Dictionary<RewardData, int> rewardResults = new Dictionary<RewardData, int>();
    private List<GameObject> instantiatedRewardTenObjects = new List<GameObject>();

    public event Action OnGachaCompleted;

    public event Action OnGachaPanelClosed;

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
        exitRewardOnceBtn.onClick.AddListener(OnGachaBackBtnClicked);
        exitRewardTenBtn.onClick.AddListener(OnGachaBackBtnClicked);
    }

    private void ResetBtn()
    {
        backBtn.onClick.RemoveAllListeners();
        buyOnceBtn.onClick.RemoveAllListeners();
        buyTenBtn.onClick.RemoveAllListeners();
        confirmYesBtn.onClick.RemoveAllListeners();
        confirmNoBtn.onClick.RemoveAllListeners();
        exitRewardOnceBtn.onClick.RemoveAllListeners();
        exitRewardTenBtn.onClick.RemoveAllListeners();

        gachaConfirmUI.SetActive(false);
        gachaOncePanelUI.SetActive(false);
        gachaTenPanelUI.SetActive(false);
        noCurrencyText.SetActive(false);
        gachaOncePanelUI.SetActive(false);
        gachaTenPanelUI.SetActive(false);

        foreach(var obj in instantiatedRewardTenObjects)
        {
            Destroy(obj);
        }
        instantiatedRewardTenObjects.Clear();
    }

    private void OnConfirm(string gachaName, int drawCount)
    {
        confirmGachaText.text = $"{gachaName}를 x{drawCount}회 돌리시겠습니까?";
        noCurrencyText.SetActive(false);
        gachaConfirmUI.SetActive(true);
    }

    private void OnBackBtnClicked()
    {
        OnGachaPanelClosed?.Invoke();

        gachaConfirmUI.SetActive(false);
        noCurrencyText.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnGachaBackBtnClicked()
    {
        gachaOncePanelUI.SetActive(false);
        gachaTenPanelUI.SetActive(false);

        rewardResults.Clear();

        if(instantiatedRewardTenObjects.Count != 0)
        {
            foreach(var obj in instantiatedRewardTenObjects)
            {
                Destroy(obj);
            }
            instantiatedRewardTenObjects.Clear();
        }
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

            ItemManager.Instance.SaveItemsAsync().Forget();

            if(drawCount == 1)
            {
                foreach(var reward in rewardResults)
                {
                    rewardNameText.text = reward.Key.RewardNameText;
                    rewardOnceText.text = $"x{reward.Value}";
                }

                gachaOncePanelUI.SetActive(true);
            }
            else if(drawCount > 1)
            {
                var instantiateCount = (rewardResults.Count - 1) / 2 + 1;

                for (int i = 0; i < instantiateCount; i++)
                {
                    var rewardTenObj = Instantiate(RewardTenPrefab, gachaTenContent);
 
                    instantiatedRewardTenObjects.Add(rewardTenObj);
                }

                int left = 0;
                int index = 0;
                foreach(var reward in rewardResults)
                {
                    var rewardTenObj = instantiatedRewardTenObjects[index];

                    if (left == 0)
                    {
                        var verticalGachaPanelUI = rewardTenObj.GetComponentInChildren<VerticalCachaPanelUI>();
                        verticalGachaPanelUI.SetLeftItem(reward.Key.RewardNameText, reward.Value);
                    }
                    else if (left == 1)
                    {
                        var verticalGachaPanelUI = rewardTenObj.GetComponentInChildren<VerticalCachaPanelUI>();
                        verticalGachaPanelUI.SetRightItem(reward.Key.RewardNameText, reward.Value);
                    }

                    left++;
                    if (left >= 2)
                    {
                        left = 0;
                        index++;
                    }
                }

                gachaTenPanelUI.SetActive(true);
            }

            gachaConfirmUI.SetActive(false);

            OnGachaCompleted?.Invoke();
        }
        else
        {
            noCurrencyText.SetActive(true);
        }
    }

    private void OnConfirmNoBtnClicked()
    {
        gachaConfirmUI.SetActive(false);
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

        CurrencyManager.Instance.SaveCurrencyAsync().Forget();

        return isEnough;
    }

    private void OnGacha()
    {
        var draws = DataTableManager.DrawTable.GetRandomDrawData(drawGroup, drawCount);

        foreach(var draw in draws)
        {
            var reward = DataTableManager.RewardTable.Get(draw.Reward_Id);
            
            UserDataMapper.AddReward(reward.RewardType, reward.Target_Id, draw.RewardQty);

            if(rewardResults.ContainsKey(reward))
            {
                if(UserDataMapper.HasPlanet(reward.Target_Id))
                {
                    rewardResults[reward] += 20;
                }
                else
                {
                    rewardResults[reward] += draw.RewardQty;
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
