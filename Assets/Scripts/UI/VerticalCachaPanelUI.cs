using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VerticalCachaPanelUI : MonoBehaviour
{
    [SerializeField] private Image leftItemIcon;
    [SerializeField] private TextMeshProUGUI leftItemText;
    [SerializeField] private TextMeshProUGUI leftItemCountText;
    [SerializeField] private Image rightItemIcon;
    [SerializeField] private TextMeshProUGUI rightItemText;
    [SerializeField] private TextMeshProUGUI rightItemCountText;

    [SerializeField] private GameObject rightItemObject;
    [SerializeField] private GameObject emptyRightItemObject;

    public void SetLeftItem(string itemName, int itemCount, string itemIconText)
    {
        leftItemText.text = itemName;
        leftItemCountText.text = $"x{itemCount}";
        leftItemIcon.sprite = LoadManager.GetLoadedGameTexture(itemIconText);
        rightItemObject.SetActive(false);
        emptyRightItemObject.SetActive(true);
    }

    public void SetRightItem(string itemName, int itemCount, string itemIconText)
    {
        rightItemText.text = itemName;
        rightItemCountText.text = $"x{itemCount}";
        rightItemIcon.sprite = LoadManager.GetLoadedGameTexture(itemIconText);
        rightItemObject.SetActive(true);
        emptyRightItemObject.SetActive(false);
    }
}
