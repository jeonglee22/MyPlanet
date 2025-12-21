using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerPanelSetttingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIconImage;

    [SerializeField] private List<Image> towerUpgradeIconImages;

    public void SetTowerPanel(int towerId)
    {
        var attackTowerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var amplifierTowerData = DataTableManager.BuffTowerTable.Get(towerId);

        if (attackTowerData != null)
        {
            towerNameText.text = attackTowerData.AttackTowerName;
            towerIconImage.sprite = LoadManager.GetLoadedGameTexture(attackTowerData.AttackTowerAsset);
        }
        else if (amplifierTowerData != null)
        {
            towerNameText.text = amplifierTowerData.BuffTowerName;
            towerIconImage.sprite = LoadManager.GetLoadedGameTexture(amplifierTowerData.BuffTowerAsset);
        }
    }

    public void SetTowerLevel(int level)
    {
        if (level < 0 || level >= 5)
            return;

        if (level == 0)
        {
            foreach (var iconImage in towerUpgradeIconImages)
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (i < level)
            {
                towerUpgradeIconImages[i].gameObject.SetActive(true);
            }
            else
            {
                towerUpgradeIconImages[i].gameObject.SetActive(false);
            }
        }

    }

    public void SetTowerNameText(string name)
    {
        towerNameText.text = name;
    }

    public void SetTowerIconImage(Sprite icon)
    {
        towerIconImage.sprite = icon;
    }
}
