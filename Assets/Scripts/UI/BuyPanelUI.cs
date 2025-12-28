using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanelUI : MonoBehaviour
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button buyBtn;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private Image needCurrencyImage;
    [SerializeField] private Image buyItemImage;
    [SerializeField] private TextMeshProUGUI failBuyText;

    [SerializeField] private RectTransform popupParent;
    [SerializeField] private GameObject popupPrefab;

    private int needItemId;
    private int buyItemId;

    private int needCurrencyValue;
    private int itemCount;

    private GameObject buyItemPanel;
    private CancellationTokenSource cts = new CancellationTokenSource();

    public event Action OnBuyCompleted;

    public void Initialize(int itemCount, int itemId, int needValue, GameObject buyItemPanel)
    {
        ResetBtn();
        backBtn.onClick.AddListener(OnBackBtnClicked);
        AddBtnSound();

        if (needValue <= 0)
        {
            var gemIconId = (int)Currency.FreeDia;
            var gemData = DataTableManager.CurrencyTable.Get(gemIconId);
            buyItemImage.sprite = LoadManager.GetLoadedGameTexture(gemData.CurrencyIconText);
            this.itemCount = itemCount;
            needCurrencyValue = 0;
            needCurrencyImage.transform.parent.gameObject.SetActive(false);
            currencyText.text = "무료";
            itemNameText.text = DataTableManager.ItemStringTable.GetString(gemData.CurrencyName);
            itemCountText.text = $"x {itemCount:N0}";

            buyItemId = gemIconId;
            this.buyItemPanel = buyItemPanel;

            failBuyText.gameObject.SetActive(false);
            buyBtn.onClick.AddListener(() => OnFreeBuyBtnClicked().Forget());

            return;
        }

        if (itemId == (int)Currency.ChargedDia)
        {
            var chargedCurrencyData = DataTableManager.CurrencyTable.Get((int)Currency.ChargedDia);
            buyItemImage.sprite = LoadManager.GetLoadedGameTexture(chargedCurrencyData.CurrencyIconText);
            this.itemCount = itemCount;
            needCurrencyValue = needValue;
            currencyText.text = needCurrencyValue.ToString("N0");
            itemNameText.text = DataTableManager.ItemStringTable.GetString(chargedCurrencyData.CurrencyName);
            itemCountText.text = $"x {itemCount:N0}";

            needItemId = (int)Currency.Gold;
            var goldData = DataTableManager.CurrencyTable.Get((int)Currency.Gold);
            needCurrencyImage.sprite = LoadManager.GetLoadedGameTexture(goldData.CurrencyIconText);
        }
        else
        {
            var itemData = DataTableManager.ItemTable.Get(itemId);

            var currencyData = DataTableManager.CurrencyTable.Get((int)Currency.Gold);
            buyItemImage.sprite = LoadManager.GetLoadedGameTexture(itemData.ItemIconText);

            needCurrencyImage.sprite = LoadManager.GetLoadedGameTexture(currencyData.CurrencyIconText);
            this.itemCount = itemCount;
            needCurrencyValue = needValue;
            currencyText.text = needCurrencyValue.ToString("N0");
            itemNameText.text = DataTableManager.ItemStringTable.GetString(itemData.ItemName);

            itemCountText.text = $"x {itemCount:N0}";

            needItemId = currencyData.Currency_Id;
        }

        failBuyText.gameObject.SetActive(false);

        buyItemId = itemId;

        if (itemId == (int)Currency.ChargedDia)
        {
            buyBtn.onClick.AddListener(() => OnChargeDiaBtnClicked().Forget());
        }
        else if (itemId == (int)ItemIds.PackageShopItem)
        {
            buyBtn.onClick.AddListener(() => OnBuyBtnClicked().Forget());
        }
        else
        {
            buyBtn.onClick.AddListener(() => OnBuyBtnClicked().Forget());
        }

        this.buyItemPanel = buyItemPanel;
    }

    private async UniTaskVoid OnFreeBuyBtnClicked()
    {
        buyBtn.interactable = false;

        UserData.FreeDia += itemCount;

        await CurrencyManager.Instance.SaveCurrencyAsync();

        OnBuyCompleted?.Invoke();

        buyBtn.interactable = true;

        gameObject.SetActive(false);
        var dailyButton = buyItemPanel.GetComponent<DailyButton>();
        dailyButton.LockedItem();

        if (popupParent.childCount > 0)
                Destroy(popupParent.GetChild(0).gameObject);
        var popup = Instantiate(popupPrefab, popupParent);
        var popupUI = popup.GetComponent<PopUpAndDestroyPanel>();
        popupUI.SetMessage("구매 완료!");
    }

    private async UniTaskVoid OnChargeDiaBtnClicked()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        var userGoldCurrency = UserData.Gold;
        if (userGoldCurrency <= needCurrencyValue)
        {
            failBuyText.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cts.Token);
            failBuyText.gameObject.SetActive(false);
            return;
        }
        else
        {
            buyBtn.interactable = false;

            UserData.Gold -= needCurrencyValue;
            UserData.ChargedDia += itemCount;

            await ItemManager.Instance.SaveItemsAsync();
            await CurrencyManager.Instance.SaveCurrencyAsync();

            OnBuyCompleted?.Invoke();

            buyBtn.interactable = true;

            gameObject.SetActive(false);

            if (popupParent.childCount > 0)
                Destroy(popupParent.GetChild(0).gameObject);
            var popup = Instantiate(popupPrefab, popupParent);
            var popupUI = popup.GetComponent<PopUpAndDestroyPanel>();
            popupUI.SetMessage("구매 완료!");
        }
    }

    private void AddBtnSound()
    {
        backBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        buyBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
    }

    private void ResetBtn()
    {
        backBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.RemoveAllListeners();
    }

    private void OnBackBtnClicked()
    {
        gameObject.SetActive(false);
    }

    private async UniTaskVoid OnBuyBtnClicked()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        var userGoldCurrency = UserData.Gold;
        if (userGoldCurrency <= needCurrencyValue)
        {
            failBuyText.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cts.Token);
            failBuyText.gameObject.SetActive(false);
            return;
        }
        else
        {
            UserData.Gold -= needCurrencyValue;
            ItemManager.Instance.AddItem(buyItemId, itemCount);

            buyBtn.interactable = false;

            await ItemManager.Instance.SaveItemsAsync();
            await CurrencyManager.Instance.SaveCurrencyAsync();

            OnBuyCompleted?.Invoke();

            buyBtn.interactable = true;

            gameObject.SetActive(false);
            var dailyButton = buyItemPanel.GetComponent<DailyButton>();
            dailyButton.LockedItem();

            if (popupParent.childCount > 0)
                Destroy(popupParent.GetChild(0).gameObject);
            var popup = Instantiate(popupPrefab, popupParent);
            var popupUI = popup.GetComponent<PopUpAndDestroyPanel>();
            popupUI.SetMessage("구매 완료!");
        }
    }
}
