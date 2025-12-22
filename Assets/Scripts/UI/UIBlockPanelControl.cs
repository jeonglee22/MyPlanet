using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBlockPanelControl : MonoBehaviour
{
    [SerializeField] private List<Button> blockButtons;
    [SerializeField] private List<RectTransform> blockRects;

    public static bool IsBlockedPanel = false;

    private void OnEnable()
    {
        IsBlockedPanel = true;
        // foreach (var btn in blockButtons)
        // {
        //     btn.interactable = false;
        // }
    }

    private void OnDisable()
    {
        IsBlockedPanel = false;
        // foreach (var btn in blockButtons)
        // {
        //     btn.interactable = true;
        // }
    }
}
