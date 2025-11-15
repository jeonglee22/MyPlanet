using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerInfoUI : PopUpUI
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TowerInstallControl installControl;

    private int infoIndex = -1;
    private bool isSameTower;

    public void SetInfo(int index)
    {
        Debug.Log("Tower Info");
        nameText.text = $"Tower {index}";

        if (installControl == null)
        {
            nameText.text = "No data";
            return;
        }

        var data = installControl.GetTowerData(index);
        if (data == null)
        {
            nameText.text = $"Empty Slot {index}";
            return;
        }

        nameText.text = $"{data.towerId}";

        isSameTower = (infoIndex == index);
        infoIndex = index;
    }

    protected override void Update()
    {
        var touchScreen = Touchscreen.current;
        if (touchScreen == null) return;

        var primary = touchScreen.primaryTouch;
        if (!primary.press.isPressed) return;

        var touchPos = primary.position.ReadValue();
        if(RectTransformUtility.RectangleContainsScreenPoint(installControl.Towers[infoIndex].GetComponent<RectTransform>(),touchPos))
        {
            return;
        }
        
        base.Update();
    }
    
    public void OnCloseInfoClicked()
    {
        gameObject.SetActive(false);
    }
}
