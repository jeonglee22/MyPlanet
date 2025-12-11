using TMPro;
using UnityEngine;

public class VerticalCachaPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI leftItemText;
    [SerializeField] private TextMeshProUGUI leftItemCountText;
    [SerializeField] private TextMeshProUGUI rightItemText;
    [SerializeField] private TextMeshProUGUI rightItemCountText;

    [SerializeField] private GameObject rightItemObject;
    [SerializeField] private GameObject emptyRightItemObject;

    public void SetLeftItem(string itemName, int itemCount)
    {
        leftItemText.text = itemName;
        leftItemCountText.text = $"x{itemCount}";
        rightItemObject.SetActive(false);
        emptyRightItemObject.SetActive(true);
    }

    public void SetRightItem(string itemName, int itemCount)
    {
        rightItemText.text = itemName;
        rightItemCountText.text = $"x{itemCount}";
        rightItemObject.SetActive(true);
        emptyRightItemObject.SetActive(false);
    }
}
