using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerDamageInfoUI : MonoBehaviour
{
    [SerializeField] private List<Image> towerImages;
    [SerializeField] private List<TextMeshProUGUI> towerDamageTexts;
    [SerializeField] private List<TextMeshProUGUI> towerDamagePercentTexts;
    [SerializeField] private List<TextMeshProUGUI> towerNames;
    [SerializeField] private List<Slider> towerDamagePercentSliders;
    [SerializeField] private List<GameObject> damageInfoObjects;
    [SerializeField] private RectTransform infoPanel;
    
    public void OnClosePanelClicked()
    {
        gameObject.SetActive(false);
    }

    public void SetTowerDamageInfos(Planet planet)
    {
        var towerCount = planet.TowerCount;
        var attackTowers = new List<TowerAttack>();
        for (int i = 0; i < towerCount; i++)
        {
            var tower = planet.GetAttackTowerToAmpTower(i);
            if (tower is TowerAttack attackTower)
            {
                attackTowers.Add(attackTower);
            }
        }

        var totalDamage = CalculateTotalDamage(attackTowers);

        for (int i = 0; i < attackTowers.Count; i++)
        {
            var tower = attackTowers[i];
            var towerData = tower.AttackTowerData;
            var towerId = towerData.towerIdInt;
            var towerTableData = DataTableManager.AttackTowerTable.GetById(towerId);
            var towerAsset = towerTableData.AttackTowerAsset;
            var towerSprite = LoadManager.GetLoadedGameTexture(towerAsset);

            towerImages[i].sprite = towerSprite;
            towerNames[i].text = towerTableData.AttackTowerName;
            towerDamageTexts[i].text = tower.TotalDamageDealt.ToString("N0");
            towerDamagePercentTexts[i].text = (tower.TotalDamageDealt * 100f / totalDamage).ToString("F2") + "%";
            towerDamagePercentSliders[i].value = tower.TotalDamageDealt / totalDamage;

            damageInfoObjects[i].SetActive(true);
        }

        for (int i = attackTowers.Count; i < towerImages.Count; i++)
        {
            damageInfoObjects[i].SetActive(false);
        }
    }

    private float CalculateTotalDamage(List<TowerAttack> attackTowers)
    {
        var damageSum = 0f;
        foreach (var tower in attackTowers)
        {
            damageSum += tower.TotalDamageDealt;
        }
        return damageSum > 0f ? damageSum : 0.00001f;
    }

    private void Update()
    {
        if (!TouchManager.Instance.IsTouching)
        {
            return;
        }

        var touchPos = TouchManager.Instance.TouchPos;
        if (!RectTransformUtility.RectangleContainsScreenPoint(infoPanel, touchPos))
        {
            gameObject.SetActive(false);
        }
    }
}
