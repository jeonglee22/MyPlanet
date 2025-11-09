using System;
using TMPro;
using UnityEngine;

public class TowerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetInfo(int index)
    {
        Debug.Log("Tower Info");
        nameText.text = $"Tower {index}";
    }
    
    public void OnCloseInfoClicked()
    {
        gameObject.SetActive(false);
    }
}
