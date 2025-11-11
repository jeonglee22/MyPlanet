using System;
using TMPro;
using UnityEngine;

public class TowerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TowerInstallControl installControl;

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
    }
    
    public void OnCloseInfoClicked()
    {
        gameObject.SetActive(false);
    }
}
