using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private Image requiredCurrencyIcon;

    private string itemName;
    private int buyitemId;
    public int BuyItemId => buyitemId;
    private int randomRewardId;
    public int RandomRewardId => randomRewardId;
    private int needItemId;
    private int needCurrencyValue;
    private int itemCount;

    private int buttonIndex = -1;
    public int ButtonIndex => buttonIndex;

    public event Action<(int, int, int, GameObject)> OnGachaButtonClicked;

    public void Initialize(int index, Action<(int, int, int, GameObject)> onClickCallback, List<int> existingItemKeys, bool isBought)
    {
        if (index < 0)
            return;

        buttonIndex = index;

        var image = LoadManager.GetLoadedGameTexture("StarDust_icon");
        itemCount = 1;
        var currencyGoldData = DataTableManager.CurrencyTable.Get((int)Currency.Gold);

        switch (index)
        {
            case 0:
                var freeDiaId = (int)Currency.FreeDia;
                var freeDiaData = DataTableManager.CurrencyTable.Get(freeDiaId);
                var freeDiaName = DataTableManager.ItemStringTable.GetString(freeDiaData.CurrencyName);

                image = LoadManager.GetLoadedGameTexture(freeDiaData.CurrencyIconText);
                requiredCurrencyIcon.transform.parent.gameObject.SetActive(false);
                itemName = freeDiaName;
                needCurrencyValue = 0;
                itemCount = 15;

                buyitemId = freeDiaId;
                needItemId = 0;
                break;
            case 1:
                var towerUpgradeId = (int)ItemIds.TowerUpgradeItem;
                var towerUpgradeData = DataTableManager.ItemTable.Get(towerUpgradeId);
                var towerUpgradeName = DataTableManager.ItemStringTable.GetString(towerUpgradeData.ItemName);

                image = LoadManager.GetLoadedGameTexture(towerUpgradeData.ItemIconText);
                itemName = towerUpgradeName;
                requiredCurrencyIcon.sprite = LoadManager.GetLoadedGameTexture(currencyGoldData.CurrencyIconText);
                needCurrencyValue = 40000;
                itemCount = 10;

                buyitemId = towerUpgradeId;
                needItemId = (int)Currency.Gold;
                break;
            case 2:
                var planetUpgradeId = (int)ItemIds.PlanetUpgradeItem;
                var planetUpgradeData = DataTableManager.ItemTable.Get(planetUpgradeId);
                var planetUpgradeName = DataTableManager.ItemStringTable.GetString(planetUpgradeData.ItemName);

                image = LoadManager.GetLoadedGameTexture(planetUpgradeData.ItemIconText);
                itemName = planetUpgradeName;
                requiredCurrencyIcon.sprite = LoadManager.GetLoadedGameTexture(currencyGoldData.CurrencyIconText);
                needCurrencyValue = 45000;
                itemCount = 10;

                buyitemId = planetUpgradeId;
                needItemId = (int)Currency.Gold;
                break;
            default:
                if (isBought)
                {
                    var boughtData = UserShopItemManager.Instance.BuyedShopItemData;
                    var existingItemData = boughtData.buyedItems[index];
                    var boughtId = existingItemData.itemId;
                    var boughtCount = existingItemData.count;

                    var rewardItemData = DataTableManager.RewardTable.GetFromTargetId(boughtId);
                    var randomRewardData = DataTableManager.DailyRerollTable.GetFromRewardId(rewardItemData.Reward_Id);

                    var rewardName = DataTableManager.ItemStringTable.GetString(rewardItemData.RewardName);
                    var currencyGroup = randomRewardData.CurrencyGroup;
                    var currencyData = DataTableManager.CurrencyTable.GetByGroup(currencyGroup);

                    itemName = rewardName;
                    requiredCurrencyIcon.sprite = LoadManager.GetLoadedGameTexture(currencyData.CurrencyIconText);

                    itemCount = boughtCount;
                    needCurrencyValue = randomRewardData.NeedCurrencyValue * itemCount;
                    buyitemId = boughtId;
                    needItemId = currencyData.Currency_Id;
                    randomRewardId = randomRewardData.DailyReroll_Id;
                    image = LoadManager.GetLoadedGameTexture(DataTableManager.ItemTable.Get(buyitemId).ItemIconText);
                }
                else
                {
                    var boughtData = UserShopItemManager.Instance.BuyedShopItemData;
                    var existingItemData = boughtData.buyedItems[index];
                    var boughtId = existingItemData.itemId;
                    var boughtCount = existingItemData.count;

                    string rewardName;
                    DailyRerollData randomRewardData;
                    CurrencyData currencyData;
                    RewardData rewardItemData;

                    if (boughtId == 0 || boughtCount == 0)
                    {
                        randomRewardData = DataTableManager.DailyRerollTable.GetRandomDataExceptKeys(existingItemKeys);
                        rewardItemData = DataTableManager.RewardTable.Get(randomRewardData.Reward_Id);
                        rewardName = DataTableManager.ItemStringTable.GetString(rewardItemData.RewardName);
                        var currencyGroup = randomRewardData.CurrencyGroup;
                        currencyData = DataTableManager.CurrencyTable.GetByGroup(currencyGroup);

                        itemCount = DataTableManager.DailyRerollTable.GetRandomCountInId(randomRewardData.DailyReroll_Id);
                        buyitemId = rewardItemData.Target_Id;

                        boughtData.buyedItems[index] = new BuyItemData(buyitemId, itemCount);
                        UserShopItemManager.Instance.SaveUserShopItemDataAsync(boughtData).Forget();
                    }
                    else
                    {
                        rewardItemData = DataTableManager.RewardTable.GetFromTargetId(boughtId);
                        randomRewardData = DataTableManager.DailyRerollTable.GetFromRewardId(rewardItemData.Reward_Id);

                        rewardName = DataTableManager.ItemStringTable.GetString(rewardItemData.RewardName);
                        var currencyGroup = randomRewardData.CurrencyGroup;
                        currencyData = DataTableManager.CurrencyTable.GetByGroup(currencyGroup);

                        itemCount = boughtCount;
                        buyitemId = boughtId;
                    }                    

                    itemName = rewardName;
                    requiredCurrencyIcon.sprite = LoadManager.GetLoadedGameTexture(currencyData.CurrencyIconText);
                    
                    needCurrencyValue = randomRewardData.NeedCurrencyValue * itemCount;

                    needItemId = currencyData.Currency_Id;
                    randomRewardId = randomRewardData.DailyReroll_Id;
                    image = LoadManager.GetLoadedGameTexture(DataTableManager.ItemTable.Get(buyitemId).ItemIconText);
                }
                break;
        }

        SetPanel(itemName, image, needCurrencyValue, itemCount);

        OnGachaButtonClicked += onClickCallback;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);

        if (isBought)
            soldOutOverlay.SetActive(true);
        else
            soldOutOverlay.SetActive(false);
    }

    private void SetPanel(string name, Sprite image, int price, int number)
    {
        nameText.text = name;
        iconImage.sprite = image;
        if (price <= 0)
        {
            priceText.text = "무료";
        }
        else
        {
            priceText.text = price.ToString("N0");
        }
        numberText.text = $"x{number:N0}";
    }

    private void OnButtonClick()
    {
        OnGachaButtonClicked?.Invoke((itemCount, buyitemId, needCurrencyValue, this.gameObject));
    }

    public void LockedItem()
    {
        soldOutOverlay.SetActive(true);
    }

    public void RefreshObj(int index, Action<(int, int, int, GameObject)> onButtonClick, List<Transform> dailyItemParents, List<int> existingItemKeys)
    {
        var dailyItemKeys = new List<int>();
        foreach (var parent in dailyItemParents)
        {
            var dailyButton = parent.GetComponentInChildren<DailyButton>();
            if (dailyButton != null)
            {
                dailyItemKeys.Add(dailyButton.RandomRewardId);
            }
        }

        var totalExceptKeys = new List<int>(dailyItemKeys);
        totalExceptKeys.AddRange(existingItemKeys);

        var image = LoadManager.GetLoadedGameTexture("StarDust_icon");
        var randomRewardData = DataTableManager.DailyRerollTable.GetRandomDataExceptKeys(totalExceptKeys);
        var rewardItemData = DataTableManager.RewardTable.Get(randomRewardData.Reward_Id);
        var rewardName = DataTableManager.ItemStringTable.GetString(rewardItemData.RewardName);
        var currencyGroup = randomRewardData.CurrencyGroup;
        var currencyData = DataTableManager.CurrencyTable.GetByGroup(currencyGroup);

        itemName = rewardName;
        requiredCurrencyIcon.sprite = LoadManager.GetLoadedGameTexture(currencyData.CurrencyIconText);
        
        itemCount = DataTableManager.DailyRerollTable.GetRandomCountInId(randomRewardData.DailyReroll_Id);
        needCurrencyValue = randomRewardData.NeedCurrencyValue * itemCount;

        buyitemId = rewardItemData.Target_Id;
        needItemId = currencyData.Currency_Id;
        randomRewardId = randomRewardData.DailyReroll_Id;

        var boughtData = UserShopItemManager.Instance.BuyedShopItemData;
        boughtData.buyedItems[index] = new BuyItemData(buyitemId, itemCount);
        UserShopItemManager.Instance.SaveUserShopItemDataAsync(boughtData).Forget();

        SetPanel(itemName, image, needCurrencyValue, itemCount);

        OnGachaButtonClicked -= onButtonClick;
        OnGachaButtonClicked += onButtonClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);

        soldOutOverlay.SetActive(false);
    }
}
