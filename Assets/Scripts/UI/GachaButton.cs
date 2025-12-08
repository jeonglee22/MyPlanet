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

    public void Initialize(int drawGroup, string name)
    {
        gachaName = name;
        nameText.text = name;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Debug.Log($"Item {gachaName} button clicked.");
    }
}
