using System;
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
    private int drawGroup;
    private int needCurrencyValue;

    public event Action<(int, int, string)> OnGachaButtonClicked;

    public void Initialize(int index, Action<(int, int, string)> onClickCallback)
    {
        if (index < 0)
            return;

        var image = LoadManager.GetLoadedGameTexture("StarDust_icon");
        var itemCount = 1;
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
                break;
            default:
                var randomRewardData = DataTableManager.DailyRerollTable.GetRandomData();
                var rewardItemData = DataTableManager.RewardTable.Get(randomRewardData.Reward_Id);
                var rewardName = DataTableManager.ItemStringTable.GetString(rewardItemData.RewardName);
                var currencyGroup = randomRewardData.CurrencyGroup;
                var currencyData = DataTableManager.CurrencyTable.GetByGroup(currencyGroup);

                itemName = rewardName;
                requiredCurrencyIcon.sprite = LoadManager.GetLoadedGameTexture(currencyData.CurrencyIconText);
                needCurrencyValue = randomRewardData.NeedCurrencyValue;
                itemCount = DataTableManager.DailyRerollTable.GetRandomCountInId(randomRewardData.DailyReroll_Id);
                break;
        }

        SetPanel(itemName, image, needCurrencyValue, itemCount);

        // nameText.text = needCurrencyValue.ToString();
        // numberText.text = "x1"; // Example, set the number of items
        // priceText.text = needCurrencyValue.ToString();
        // iconImage.sprite = null; // Set appropriate sprite based on index or data

        OnGachaButtonClicked += onClickCallback;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);

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
            priceText.text = price.ToString();
        }
        numberText.text = $"x{number}";
    }

    private void OnButtonClick()
    {
        OnGachaButtonClicked?.Invoke((needCurrencyValue, drawGroup, itemName));
    }
}
