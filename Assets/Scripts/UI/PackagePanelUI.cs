using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackagePanelUI : MonoBehaviour
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button buyBtn;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI[] itemCountText;
    [SerializeField] private Image needCurrencyImage;
    [SerializeField] private Image[] buyItemImage;
    [SerializeField] private TextMeshProUGUI failBuyText;

    [SerializeField] private GameObject[] itemSlots;
    [SerializeField] private GameObject[] itemLines;

    [SerializeField] private RectTransform popupParent;
    [SerializeField] private GameObject popupPrefab;

    private int needItemId;
    private int buyItemId;

    private int needCurrencyValue;
    private int itemCount;

    private GameObject buyItemPanel;
    private CancellationTokenSource cts = new CancellationTokenSource();

    public event Action OnBuyCompleted;

    public void Initialize(int itemCount, int packageId, int needValue, GameObject buyItemPanel)
    {
        ResetBtn();
        backBtn.onClick.AddListener(OnBackBtnClicked);
        AddBtnSound();
        // var itemData = DataTableManager.ItemTable.Get(packageId);
        var packageData = PackageItems.PackageItemDict[packageId];

        var currencyData = DataTableManager.CurrencyTable.Get((int)Currency.Gold);
        int index = 0;

        var buyItemCount = packageData.Count;
        SetItemGrid(buyItemCount);

        foreach (var item in packageData.Values)
        {
            var itemData = DataTableManager.ItemTable.Get(item.Item1);
            var getCurrencyData = DataTableManager.CurrencyTable.Get(item.Item1);
            if (itemData != null)
            {
                buyItemImage[index].sprite = LoadManager.GetLoadedGameTexture(itemData.ItemIconText);
                itemCountText[index].text = $"x {item.Item2:N0}";
            }
            else if (getCurrencyData != null)
            {
                buyItemImage[index].sprite = LoadManager.GetLoadedGameTexture(getCurrencyData.CurrencyIconText);
                itemCountText[index].text = $"x {item.Item2:N0}";
            }

            index++;
        }

        needCurrencyImage.sprite = LoadManager.GetLoadedGameTexture(currencyData.CurrencyIconText);
        this.itemCount = itemCount;
        needCurrencyValue = needValue;
        currencyText.text = needCurrencyValue.ToString();
        itemNameText.text = PackageItems.PackageName[packageId];

        needItemId = currencyData.Currency_Id;

        failBuyText.gameObject.SetActive(false);

        buyItemId = packageId;

        buyBtn.onClick.AddListener(() => OnBuyBtnClicked().Forget());

        this.buyItemPanel = buyItemPanel;
    }

    private void SetItemGrid(int itemCount)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].SetActive(false);
        }
        for (int i = 0; i < itemLines.Length; i++)
        {
            itemLines[i].SetActive(false);
        }

        switch (itemCount)
        {
            case 1:
            case 2:
                for (int i = 0; i < itemCount; i++)
                {
                    itemSlots[i].SetActive(true);
                }
                itemLines[0].SetActive(true);
                break;
            case 3:
            case 4:
                for (int i = 0; i < itemCount; i++)
                {
                    itemSlots[i].SetActive(true);
                }
                itemLines[0].SetActive(true);
                itemLines[1].SetActive(true);
                break;
            default:
                break;
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
            buyBtn.interactable = false;
            UserData.Gold -= needCurrencyValue;

            for (int i = 0; i < PackageItems.PackageItemDict[buyItemId].Count; i++)
            {
                var item = PackageItems.PackageItemDict[buyItemId][i];
                if (item.Item1 == (int)Currency.Gold)
                {
                    UserData.Gold += item.Item2;
                    continue;
                }
                else if (item.Item1 == (int)Currency.ChargedDia)
                {
                    UserData.ChargedDia += item.Item2;
                    continue;
                }
                else if (item.Item1 == (int)Currency.FreeDia)
                {
                    UserData.FreeDia += item.Item2;
                    continue;
                }

                ItemManager.Instance.AddItem(item.Item1, item.Item2);
            }            

            await ItemManager.Instance.SaveItemsAsync();
            await CurrencyManager.Instance.SaveCurrencyAsync();

            OnBuyCompleted?.Invoke();

            buyBtn.interactable = true;

            gameObject.SetActive(false);
            var packageItemButton = buyItemPanel.GetComponent<PackageItemButton>();
            packageItemButton.LockedItem();

            var currentShopData = UserShopItemManager.Instance.BuyedShopItemData;
            currentShopData.packageShop = true;
            await UserShopItemManager.Instance.SaveUserShopItemDataAsync(currentShopData);

            if (popupParent.childCount > 0)
                Destroy(popupParent.GetChild(0).gameObject);
            var popup = Instantiate(popupPrefab, popupParent);
            var popupUI = popup.GetComponent<PopUpAndDestroyPanel>();
            popupUI.SetMessage("구매 완료!");
        }
    }
}
