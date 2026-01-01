using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerDragImageUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    public Image IconImage => iconImage;

    [SerializeField] private List<Image> upgradeStars;

    public void UpdateUpgradeStars(int reinforceLevel)
    {
        if(upgradeStars == null || upgradeStars.Count == 0)
        {
            return;
        }

        for(int i = 0; i < upgradeStars.Count; i++)
        {
            upgradeStars[i].gameObject.SetActive(false);
        }

        int activeStarCount = Mathf.Min(reinforceLevel, upgradeStars.Count);
        for(int i = 0; i < activeStarCount; i++)
        {
            upgradeStars[i].gameObject.SetActive(true);
        }
    }
}