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
    }

    private void OnButtonClick()
    {
        OnGachaButtonClicked?.Invoke((needCurrencyValue, drawGroup, gachaName));
    }
}
