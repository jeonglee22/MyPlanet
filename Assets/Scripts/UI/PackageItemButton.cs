using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageItemButton : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;
    // [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private GameObject soldOutOverlay;
    // [SerializeField] private Image requiredCurrencyIcon;

    private string itemName;
    private Sprite image;
    private int buyitemId;
    private int needItemId;
    private int needCurrencyValue;
    private int itemCount;

    public event Action<(int, int, int, GameObject)> OnBuyButtonClicked;

    public void Initialize(int index, Action<(int, int, int, GameObject)> onClickCallback)
    {
        if (index < 0)
            return;
        

        var currencyGoldData = DataTableManager.CurrencyTable.Get((int)Currency.Gold);
        var currencyChargedDiaData = DataTableManager.CurrencyTable.Get((int)Currency.ChargedDia);

        switch (index)
        {
            case (int)PackageType.StarterPack:
                var package = PackageItems.NewPlayerPackage1;

                image = LoadManager.GetLoadedGameTexture("WhiteChest");
                itemName = PackageItems.PackageName[(int)PackageType.StarterPack];

                needCurrencyValue = 1000;

                buyitemId = (int)PackageType.StarterPack;
                needItemId = 1;
                break;
            default:
                break;
        }

        SetPanel(itemName, image, needCurrencyValue);

        OnBuyButtonClicked += onClickCallback;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);

        soldOutOverlay.SetActive(false);
    }

    private void SetPanel(string name, Sprite image, int price)
    {
        nameText.text = name;
        iconImage.sprite = image;
 
        var numberFormat = new CultureInfo("ko-KR", false).NumberFormat;
        priceText.text = price.ToString("c", numberFormat);

        // numberText.text = $"x{number}";
    }

    private void OnButtonClick()
    {
        OnBuyButtonClicked?.Invoke((0, buyitemId, needCurrencyValue*100, this.gameObject));
    }

    public void LockedItem()
    {
        soldOutOverlay.SetActive(true);
    }
}
