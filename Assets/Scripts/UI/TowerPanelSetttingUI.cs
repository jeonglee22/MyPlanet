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
            var attackTowerExplainId = attackTowerData.TowerText_ID;
            var attackTowerExplainData = DataTableManager.TowerExplainTable.Get(attackTowerExplainId);

            towerNameText.text = attackTowerExplainData.TowerName;
            towerIconImage.sprite = LoadManager.GetLoadedGameTexture(attackTowerData.AttackTowerAsset);
        }
        else if (amplifierTowerData != null)
        {
            var amplifierTowerExplainId = amplifierTowerData.TowerText_ID;
            var amplifierTowerExplainData = DataTableManager.TowerExplainTable.Get(amplifierTowerExplainId);

            towerNameText.text = amplifierTowerExplainData.TowerName;
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
