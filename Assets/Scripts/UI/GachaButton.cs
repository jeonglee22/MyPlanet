using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private List<Sprite> icons;

    private string gachaName;
    private int drawGroup;
    private int needCurrencyValue;

    public event Action<(int, int, string)> OnGachaButtonClicked;

    public void Initialize(int needCurrencyValue, int drawGroup, string name, Action<(int, int, string)> onClickCallback)
    {
        gachaName = name;
        nameText.text = needCurrencyValue.ToString();
        this.drawGroup = drawGroup;
        this.needCurrencyValue = needCurrencyValue;

        OnGachaButtonClicked += onClickCallback;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);

        switch ((CurrencyType)drawGroup)
        {
            case CurrencyType.Gold:
                iconImage.sprite = icons[0];
                break;
            case CurrencyType.FreeDia:
                iconImage.sprite = icons[1];
                break;
            case CurrencyType.FreePlusChargedDia:
                iconImage.sprite = icons[2];
                break;
            case CurrencyType.ChargedDia:
                iconImage.sprite = icons[3];
                break;
        }
    }

    private void OnButtonClick()
    {
        OnGachaButtonClicked?.Invoke((needCurrencyValue, drawGroup, gachaName));
    }
}
