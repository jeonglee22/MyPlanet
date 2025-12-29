using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class ChargeDiaButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI numberText;
    // [SerializeField] private GameObject soldOutOverlay;
    // [SerializeField] private Image requiredCurrencyIcon;

    private string itemName;
    private Sprite image;
    private int buyitemId;
    private int needItemId;
    private int needCurrencyValue;
    private int itemCount;

    private const int FirstPrice = 1000;
    private const int FirstAmount = 1000;
    private const int SecondPrice = 5000;
    private const int SecondAmount = 5500;
    private const int ThirdPrice = 10000;
    private const int ThirdAmount = 12000;

    public event Action<(int, int, int, GameObject)> OnGachaButtonClicked;

    public void Initialize(int index, Action<(int, int, int, GameObject)> onClickCallback)
    {
        if (index < 0)
            return;
        

        itemCount = 1;
        var currencyGoldData = DataTableManager.CurrencyTable.Get((int)Currency.Gold);
        var currencyChargedDiaData = DataTableManager.CurrencyTable.Get((int)Currency.ChargedDia);

        switch (index)
        {
            case 0:
                image = LoadManager.GetLoadedGameTexture(currencyChargedDiaData.CurrencyIconText);
                itemName = DataTableManager.ItemStringTable.GetString(currencyChargedDiaData.CurrencyName);
                needCurrencyValue = FirstPrice;
                itemCount = FirstAmount;

                buyitemId = (int)Currency.ChargedDia;
                needItemId = 1;
                break;
            case 1:

                image = LoadManager.GetLoadedGameTexture(currencyChargedDiaData.CurrencyIconText);
                itemName = DataTableManager.ItemStringTable.GetString(currencyChargedDiaData.CurrencyName);
                needCurrencyValue = SecondPrice;
                itemCount = SecondAmount;

                buyitemId = (int)Currency.ChargedDia;
                needItemId = 1;
                break;
            case 2:

                image = LoadManager.GetLoadedGameTexture(currencyChargedDiaData.CurrencyIconText);
                itemName = DataTableManager.ItemStringTable.GetString(currencyChargedDiaData.CurrencyName);
                needCurrencyValue = ThirdPrice;
                itemCount = ThirdAmount;

                buyitemId = (int)Currency.ChargedDia;
                needItemId = 1;
                break;
            default:
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

        // soldOutOverlay.SetActive(false);
    }

    private void SetPanel(string name, Sprite image, int price, int number)
    {
        nameText.text = name;
        iconImage.sprite = image;
 
        var numberFormat = new CultureInfo("ko-KR", false).NumberFormat;
        priceText.text = price.ToString("c", numberFormat);

        numberText.text = $"x{number}";
    }

    private void OnButtonClick()
    {
        OnGachaButtonClicked?.Invoke((itemCount, buyitemId, needCurrencyValue*100, this.gameObject));
    }
}
