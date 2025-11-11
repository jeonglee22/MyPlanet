using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject[] upgradeUIs;

    //test
    private Color[] towerColor;
    public Color choosedColor { get; private set; }

    private void Start()
    {
        towerColor = new Color[3] {Color.yellow, Color.green, Color.cyan};
    }
    
    public void OnClickUpgradeUIClicked(int index)
    {
        Debug.Log(index);
        var color = towerColor[index];
        choosedColor = color;
        upgradeUIs[index].GetComponentInChildren<Image>().color = choosedColor;
        upgradeUIs[(index + 1) % 3].GetComponentInChildren<Image>().color = Color.white;
        upgradeUIs[(index + 2) % 3].GetComponentInChildren<Image>().color = Color.white;
    }
}