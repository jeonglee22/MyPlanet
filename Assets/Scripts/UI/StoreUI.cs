using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private DailyShopResetTimer dailyShopResetTimer;

    [SerializeField] private Button backBtn;
    
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject shopCategory;
    [SerializeField] private GameObject dailyCategory;
    [SerializeField] private GameObject chargeDiaCategory;
    [SerializeField] private GameObject packageCategory;

    [SerializeField] private GameObject gachaButtonPrefab;
    [SerializeField] private GameObject dailyButtonPrefab;
    [SerializeField] private GameObject chargeDiaButtonPrefab;
    [SerializeField] private GameObject packageButtonPrefab;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private GachaPanelUI gachaPanelUI;
    [SerializeField] private BuyPanelUI buyPanelUI;
    [SerializeField] private PackagePanelUI packagePanelUI;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI freeDiaText;
    [SerializeField] private TextMeshProUGUI chargedDiaText;

    [SerializeField] private Button inventoryBtn;
    [SerializeField] private GameObject inventory;
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject inventoryItemPrefab;
    private List<GameObject> instantiatedInventoryItems = new List<GameObject>();

    [SerializeField] private TextMeshProUGUI dailyRefreshTimeText;
    [SerializeField] private Button dailyRefreshButton;
    [SerializeField] private TextMeshProUGUI dailyRefreshCostText;

    [SerializeField] private RectTransform popupParent;
    [SerializeField] private GameObject popupPrefab;

    private bool isFirstRefresh = true;
    private int dailyRefreshCost = 50;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => CurrencyManager.Instance.IsInitialized && ItemManager.Instance.IsInitialized);

        backBtn.onClick.AddListener(OnBackBtnClicked);
        gachaPanelUI.gameObject.SetActive(false);
        InitializeShop();

        UpdateCurrencyUI();
        gachaPanelUI.OnGachaCompleted += UpdateCurrencyUI;
        buyPanelUI.OnBuyCompleted += UpdateCurrencyUI;
        packagePanelUI.OnBuyCompleted += UpdateCurrencyUI;

        inventory.SetActive(false);
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);

        InitializeInventory();

        gachaPanelUI.OnGachaPanelClosed += OnInventoryBtnActive;
        dailyShopResetTimer.OnDailyReset += RefreshShop;

        dailyRefreshButton.onClick.AddListener(() => OnDailyRefreshButtonClicked().Forget());

        var userShopData = UserShopItemManager.Instance.BuyedShopItemData;
        if (userShopData.isUsedReroll)
        {
            isFirstRefresh = false;
            dailyRefreshCost = 100;
            dailyRefreshCostText.text = "100";
        }
    }

    private void OnDestroy()
    {
        gachaPanelUI.OnGachaCompleted -= UpdateCurrencyUI;
        buyPanelUI.OnBuyCompleted -= UpdateCurrencyUI;
        packagePanelUI.OnBuyCompleted -= UpdateCurrencyUI;
        backBtn.onClick.RemoveListener(OnBackBtnClicked);
        inventoryBtn.onClick.RemoveListener(OnInventoryBtnClicked);

        gachaPanelUI.OnGachaPanelClosed -= OnInventoryBtnActive;
        dailyShopResetTimer.OnDailyReset -= RefreshShop;
    }

    private void OnBackBtnClicked()
    {
        lobbyPanel.SetActive(true);
        gachaPanelUI.gameObject.SetActive(false);
        inventory.SetActive(false);
        OnInventoryBtnActive();
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        
    }

    private void RefreshShop()
    {
        Debug.Log("Daily Shop Reset Triggered");

        isFirstRefresh = true;
        dailyRefreshCost = 50;
        dailyRefreshCostText.text = "50";

        var userShopData = UserShopItemManager.Instance.BuyedShopItemData;
        userShopData.isUsedReroll = false;
        userShopData.dailyShop = new List<bool>();
        for (int i = 0; i < 6; i++)
        {
            userShopData.dailyShop.Add(false);
        }
        UserShopItemManager.Instance.SaveUserShopItemDataAsync(userShopData).Forget();

        OnDailyRefreshButtonClicked().Forget();

        UpdateCurrencyUI();
    }
    
    private async UniTaskVoid OnDailyRefreshButtonClicked()
    {
        dailyRefreshButton.interactable = false;
        var userFreeDia = UserData.FreeDia;
        if (dailyRefreshCost > userFreeDia)
        {
            if (popupParent.childCount > 0)
                Destroy(popupParent.GetChild(0).gameObject);
            var popup = Instantiate(popupPrefab, popupParent);
            var popupUI = popup.GetComponent<PopUpAndDestroyPanel>();
            popupUI.SetMessage("무료 다이아가 부족합니다.");
            dailyRefreshButton.interactable = true;
            return;
        }

        UserData.FreeDia -= dailyRefreshCost;
        
        var result = await CurrencyManager.Instance.SaveCurrencyAsync();
        if (!result.success)
        {
            UserData.FreeDia += dailyRefreshCost;
            dailyRefreshButton.interactable = true;
            return;
        }

        UpdateCurrencyUI();

        if (isFirstRefresh)
        {
            isFirstRefresh = false;
            dailyRefreshCost = 100;
            dailyRefreshCostText.text = "100";
            var currentShopData = UserShopItemManager.Instance.BuyedShopItemData;
            currentShopData.isUsedReroll = true;
            await UserShopItemManager.Instance.SaveUserShopItemDataAsync(currentShopData);
        }

        CreateCategory(dailyCategory, ShopCategory.DailyShopRefresh, CategoryName.DailyShop, dailyButtonPrefab, OnDailyButtonClick);
        dailyRefreshButton.interactable = true;
    }

    private void InitializeShop()
    {
        // foreach(Transform child in scrollViewContent)
        // {
        //     Destroy(child.gameObject);
        // }

        CreateCategory(shopCategory, ShopCategory.Gacha, CategoryName.Gacha, gachaButtonPrefab, OnGachaButtonClick);
        CreateCategory(dailyCategory, ShopCategory.DailyShop, CategoryName.DailyShop, dailyButtonPrefab, OnDailyButtonClick);
        CreateCategory(chargeDiaCategory, ShopCategory.ChargeDiaShop, CategoryName.ChargeDiaShop, chargeDiaButtonPrefab, OnChargeDiaButtonClick);
        CreateCategory(packageCategory, ShopCategory.PackageShop, CategoryName.PackageShop, packageButtonPrefab, OnPackageButtonClick);
    }

    private void OnPackageButtonClick((int needCurrencyValue, int itemId, int needId, GameObject buyButton) info)
    {
        packagePanelUI.Initialize(info.needCurrencyValue, info.itemId, info.needId, info.buyButton);
        packagePanelUI.gameObject.SetActive(true);
    }

    private void OnChargeDiaButtonClick((int needCurrencyValue, int itemId, int needId, GameObject buyButton) info)
    {
        buyPanelUI.Initialize(info.needCurrencyValue, info.itemId, info.needId, info.buyButton);
        buyPanelUI.gameObject.SetActive(true);
    }

    private void OnDailyButtonClick((int needCurrencyValue, int itemId, int needId, GameObject buyButton) info)
    {
        buyPanelUI.Initialize(info.needCurrencyValue, info.itemId, info.needId, info.buyButton);
        buyPanelUI.gameObject.SetActive(true);
    }

    private void CreateCategory(GameObject Panel, ShopCategory category, string categoryName, GameObject buttonPrefab, Action<(int, int, string)> onButtonClick = null)
    {
        var categoryUI = Panel.GetComponent<ShopCategori>();

        categoryUI.Initialize(category, categoryName, buttonPrefab, onButtonClick);
    }

    private void CreateCategory(GameObject Panel, ShopCategory category, string categoryName, GameObject buttonPrefab, Action<(int, int, int, GameObject)> onButtonClick = null)
    {
        var categoryUI = Panel.GetComponent<ShopCategori>();

        categoryUI.Initialize(category, categoryName, buttonPrefab, onButtonClick);
    }

    private void OnGachaButtonClick((int needCurrencyValue, int drawGroup, string gachaName) info)
    {
        gachaPanelUI.Initialize(info.needCurrencyValue, info.drawGroup, info.gachaName);
        gachaPanelUI.gameObject.SetActive(true);

        inventory.SetActive(false);
        inventoryBtn.interactable = false;
    }

    private void UpdateCurrencyUI()
    {
        goldText.text = UserData.Gold.ToString();
        freeDiaText.text = $"무료 {UserData.FreeDia}";
        chargedDiaText.text = $"유료 {UserData.ChargedDia}";
    }

    private void InitializeInventory()
    {
        var items = DataTableManager.ItemTable.GetAllItemsExceptCollectionItem();
        foreach(var item in items)
        {
            var itemObj = Instantiate(inventoryItemPrefab, scrollContent);
            var itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            var itemCount = ItemManager.Instance.GetItem(item.Item_Id);
            itemText.text = $"{item.ItemNameText} : {itemCount}";
            instantiatedInventoryItems.Add(itemObj);
        }
    }

    public void OnInventoryBtnClicked()
    {
        var items = DataTableManager.ItemTable.GetAllItemsExceptCollectionItem();
        var index = 0;
        foreach(var item in items)
        {
            var itemObj = instantiatedInventoryItems[index];
            var itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            var itemCount = ItemManager.Instance.GetItem(item.Item_Id);
            itemText.text = $"{item.ItemNameText} : {itemCount}";
            index++;
        }

        inventory.SetActive(!inventory.activeSelf);
    }

    public void OnInventoryBtnActive() => inventoryBtn.interactable = true;
}
