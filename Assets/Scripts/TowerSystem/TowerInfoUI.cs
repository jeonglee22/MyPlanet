using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerInfoUI : PopUpUI
{
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private TextMeshProUGUI nameText;  //no use

    [Header("Left Panel - Tower Data")]
    [SerializeField] private TextMeshProUGUI towerIdValueText;
    [SerializeField] private TextMeshProUGUI rangeTypeValueText;
    [SerializeField] private TextMeshProUGUI rangeValueText;
    [SerializeField] private TextMeshProUGUI priorityTypeValueText;
    [SerializeField] private TextMeshProUGUI priorityOrderValueText;
    [SerializeField] private TextMeshProUGUI fireRateValueText;
    [SerializeField] private TextMeshProUGUI hitRateValueText;
    [SerializeField] private TextMeshProUGUI spreadAccuracyValueText;

    [Header("Right Panel - Projectile Data")]
    [SerializeField] private TextMeshProUGUI projectileTypeValueText;
    [SerializeField] private TextMeshProUGUI projectilePrefabValueText;
    [SerializeField] private TextMeshProUGUI hitEffectValueText;
    [SerializeField] private TextMeshProUGUI damageValueText;
    [SerializeField] private TextMeshProUGUI fixedPenetrationValueText;
    [SerializeField] private TextMeshProUGUI percentPenetrationValueText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI accelerationValueText;
    [SerializeField] private TextMeshProUGUI targetNumberValueText;
    [SerializeField] private TextMeshProUGUI lifeTimeValueText;
    [SerializeField] private TextMeshProUGUI hitRadiusValueText;

    private int infoIndex = -1;
    private bool isSameTower;

    public void SetInfo(int index)
    {
        nameText.text = $"Tower {index}";

        if (installControl == null)
        {
            nameText.text = "No data";
            SetAllText(null);
            return;
        }

        var data = installControl.GetTowerData(index);
        if (data == null)
        {
            nameText.text = $"Empty Slot {index}";
            SetAllText(null);
            return;
        }

        nameText.text = $"{data.towerId}"; //no use

        isSameTower = (infoIndex == index);
        infoIndex = index;

        //left panel_tower
        SetText(towerIdValueText, data.towerId);
        SetText(rangeTypeValueText, data.rangeData != null ? data.rangeData.RangeType.ToString() : null);
        SetText(rangeValueText, data.rangeData != null ? data.rangeData.GetRange().ToString("0.0") : null);
        SetText(priorityTypeValueText, data.targetPriority != null ? data.targetPriority.GetType().Name : null);
        SetText(priorityOrderValueText, data.targetPriority != null ? (data.targetPriority.IsDescending ? "Max" : "Min") : null);
        SetText(fireRateValueText, data.fireRate.ToString("0.00"));
        SetText(hitRateValueText, data.hitRate.ToString("0.00") + "%");
        SetText(spreadAccuracyValueText, data.spreadAccuracy.ToString("0.00") + "%");
        //right panel_projectile
        var proj = data.projectileType;
        SetText(projectileTypeValueText, proj != null ? proj.projectileType.ToString() : null);
        SetText(projectilePrefabValueText, proj != null && proj.projectilePrefab != null ? proj.projectilePrefab.name : null);
        SetText(hitEffectValueText, proj != null && proj.hitEffect != null ? proj.hitEffect.name : null);
        SetText(damageValueText, proj != null ? proj.damage.ToString("0.00") : null);
        SetText(fixedPenetrationValueText, proj != null ? proj.fixedPanetration.ToString("0.00") : null);
        SetText(percentPenetrationValueText, proj != null ? proj.percentPenetration.ToString("0.00") + "%" : null);
        SetText(speedValueText, proj != null ? proj.speed.ToString("0.00") : null);
        SetText(accelerationValueText, proj != null ? proj.acceleration.ToString("0.00") : null);
        SetText(targetNumberValueText, proj != null ? proj.targetNumber.ToString() : null);
        SetText(lifeTimeValueText, proj != null ? proj.lifeTime.ToString("0.00") : null);
        SetText(hitRadiusValueText, proj != null ? proj.hitRadius.ToString("0.00") : null);
    }

    protected override void Update()
    {
        touchPos = TouchManager.Instance.TouchPos;
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

    private void SetText(TextMeshProUGUI tmp, string value)
    {
        if (tmp == null) return;
        tmp.text = value ?? "-";
    }

    private void SetAllText(string value)
    {
        //left panel_tower
        SetText(towerIdValueText, value);
        SetText(rangeTypeValueText, value);
        SetText(rangeValueText, value);
        SetText(priorityTypeValueText, value);
        SetText(priorityOrderValueText, value);
        SetText(fireRateValueText, value);
        SetText(hitRateValueText, value);
        SetText(spreadAccuracyValueText, value);
        //right panel_projectile
        SetText(projectileTypeValueText, value);
        SetText(projectilePrefabValueText, value);
        SetText(hitEffectValueText, value);
        SetText(damageValueText, value);
        SetText(fixedPenetrationValueText, value);
        SetText(percentPenetrationValueText, value);
        SetText(speedValueText, value);
        SetText(accelerationValueText, value);
        SetText(targetNumberValueText, value);
        SetText(lifeTimeValueText, value);
        SetText(hitRadiusValueText, value);
    }
}
