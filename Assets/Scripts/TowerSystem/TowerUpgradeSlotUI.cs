using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TowerUpgradeSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject[] upgradeUIs;
    [SerializeField] private TowerInstallControl installControl;

    //test
    private Color[] towerColor;
    private List<int> numlist;
    [SerializeField] private TextMeshProUGUI[] uiTexts;
    public Color choosedColor { get; private set; }

    private void Start()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);

        towerColor = new Color[3] { Color.yellow, Color.green, Color.cyan };
    }

    private void OnEnable()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(true);

        ResetChoose();
        installControl.IsReadyInstall = false;

        numlist = new List<int>();
        foreach (var text in uiTexts)
        {
            int number;
            int count = 0;
            do
            {
                number = Random.Range(0, installControl.TowerCount);
                count++;
            } while (numlist.Contains(number) && count < installControl.TowerCount);

            numlist.Add(number);

            // test
            if (!installControl.IsUsedSlot(number))
            {
                text.text = $"new Tower";
            }
            else
            {
                text.text = $"Upgrade\n{number}";
            }
            //
        }
    }

    private void OnDisable()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OnClickUpgradeUIClicked(int index)
    {
        var currentColor = upgradeUIs[index].GetComponentInChildren<Image>().color;
        if (currentColor != Color.white)
        {
            installControl.IsReadyInstall = false;
            upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
            return;
        }

        var color = towerColor[index];
        choosedColor = color;
        upgradeUIs[index].GetComponentInChildren<Image>().color = choosedColor;
        upgradeUIs[(index + 1) % 3].GetComponentInChildren<Image>().color = Color.white;
        upgradeUIs[(index + 2) % 3].GetComponentInChildren<Image>().color = Color.white;
        installControl.IsReadyInstall = true;
        installControl.ChoosedData = (choosedColor, uiTexts[index].text);

        if (installControl.IsUsedSlot(numlist[index]))
        {
            installControl.UpgradeTower(numlist[index]);
            gameObject.SetActive(false);
        }
    }
    
    private void ResetChoose()
    {
        foreach(var ui in upgradeUIs)
        {
            ui.GetComponentInChildren<Image>().color = Color.white;
        }
    }
}
