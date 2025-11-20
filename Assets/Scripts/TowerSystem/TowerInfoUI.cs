using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerInfoUI : PopUpUI
{
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private TextMeshProUGUI nameText;  //no use

    [Header("Left Panel - Tower Data")]
    [SerializeField] private TextMeshProUGUI towerIdValueText;
    // [SerializeField] private TextMeshProUGUI rangeTypeValueText;
    [SerializeField] private TextMeshProUGUI rangeValueText;
    // [SerializeField] private TextMeshProUGUI priorityTypeValueText;
    // [SerializeField] private TextMeshProUGUI priorityOrderValueText;
    [SerializeField] private TextMeshProUGUI fireRateValueText;
    [SerializeField] private TextMeshProUGUI hitRateValueText;
    [SerializeField] private TextMeshProUGUI spreadAccuracyValueText;
    [SerializeField] private TextMeshProUGUI damageValueText;

    [Header("Right Panel - Projectile Data")]
    // [SerializeField] private TextMeshProUGUI projectileTypeValueText;
    // [SerializeField] private TextMeshProUGUI projectilePrefabValueText;
    // [SerializeField] private TextMeshProUGUI hitEffectValueText;
    [SerializeField] private TextMeshProUGUI fixedPenetrationValueText;
    [SerializeField] private TextMeshProUGUI percentPenetrationValueText;
    // [SerializeField] private TextMeshProUGUI speedValueText;
    // [SerializeField] private TextMeshProUGUI accelerationValueText;
    [SerializeField] private TextMeshProUGUI targetNumberValueText;
    [SerializeField] private TextMeshProUGUI projectileNumberValueText;
    [SerializeField] private TextMeshProUGUI lifeTimeValueText;
    [SerializeField] private TextMeshProUGUI projectileSizeValueText;

    [Header("Bottom Panel - Ability Explain")]
    [SerializeField] private TextMeshProUGUI abilityExplainText;

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

        var attackTower = installControl.GetAttackTower(index); //Attack Tower Data

        if (attackTower == null||attackTower.AttackTowerData==null) //AmpliferTower
        {
            SetAllText("증폭타워");
            infoIndex = index;
            return;
        } 

        var attackTowerData = attackTower.AttackTowerData;
        var buffedProjectile = attackTower.CurrentProjectileData;

        nameText.text = $"{attackTowerData.towerId}"; //no use

        isSameTower = (infoIndex == index);
        infoIndex = index;

        var abilities = attackTower.Abilities;
        

        //Left panel_tower----------------------------------------
        SetText(towerIdValueText, attackTowerData.towerId);
        // SetText(rangeTypeValueText, attackTowerData.rangeData != null ? attackTowerData.rangeData.RangeType.ToString() : null);
        SetText(rangeValueText, attackTowerData.rangeData != null ? attackTowerData.rangeData.GetRange().ToString("0.0") : null);
        // SetText(priorityTypeValueText, attackTowerData.targetPriority != null ? attackTowerData.targetPriority.GetType().Name : null);
        // SetText(priorityOrderValueText, attackTowerData.targetPriority != null ? (attackTowerData.targetPriority.IsDescending ? "Max" : "Min") : null);
        SetText(hitRateValueText, attackTowerData.hitRate.ToString("0.00") + "%");
        SetText(spreadAccuracyValueText, attackTowerData.spreadAccuracy.ToString("0.00") + "%");

        //Current Data
        SetText(fireRateValueText,$"{attackTower.AttackTowerData.fireRate:0.00}", attackTower.AttackTowerData.fireRate == attackTower.CurrentFireRate ? float.MaxValue : attackTower.CurrentFireRate);
        //--------------------------------------------------------

        //Right panel_projectile----------------------------------
        var proj = attackTowerData.projectileType;
        // SetText(projectileTypeValueText, proj != null ? proj.ProjectileName : null);
        //SetText(projectilePrefabValueText, proj != null && proj.projectilePrefab != null ? proj.projectilePrefab.name : null);
        //SetText(hitEffectValueText, proj != null && proj.hitEffect != null ? proj.hitEffect.name : null);
        
        //Buffed Data
        SetText(damageValueText, proj != null ? proj.Attack.ToString("0.00") : null, CalculateEachAbility(200001,abilities, proj.Attack));
        SetText(fixedPenetrationValueText, proj != null ? proj.FixedPenetration.ToString("0.00") : null,  CalculateEachAbility(200004,abilities, proj.FixedPenetration));
        SetText(percentPenetrationValueText, proj != null ? proj.RatePenetration.ToString("0.00") + "%" : null,  CalculateEachAbility(200003,abilities, proj.RatePenetration), "%");
        // SetText(speedValueText, proj != null ? proj.ProjectileSpeed.ToString("0.00") : null);
        // SetText(accelerationValueText, proj != null ? proj.ProjectileAddSpeed.ToString("0.00") : null);
        SetText(projectileNumberValueText, proj != null ? 0.ToString() : null);
        SetText(targetNumberValueText, proj != null ? proj.TargetNum.ToString() : null);
        SetText(lifeTimeValueText, proj != null ? proj.RemainTime.ToString("0.00") : null);
        SetText(projectileSizeValueText, proj != null ? proj.CollisionSize.ToString("0.00") : null, CalculateEachAbility(200006,abilities, proj.CollisionSize));
        //--------------------------------------------------------
        SetText(abilityExplainText, AbilityManager.Instance.GetAbility(abilities[0])?.ToString());
    }

    protected override void Update()
    {
        touchPos = TouchManager.Instance.TouchPos;
        if(infoIndex == -1)
            return;

        if(RectTransformUtility.RectangleContainsScreenPoint(installControl.Towers[infoIndex].GetComponent<RectTransform>(),touchPos))
        {
            return;
        }
        
        base.Update();
    }

    private float CalculateEachAbility(int abilityId, List<int> abilities, float baseValue)
    {
        int count = abilities.FindAll(x => x == abilityId).Count;
        return CalculateAbilityUpgradeValue(abilityId, count, baseValue);
    }

    private float CalculateAbilityUpgradeValue(int abilityId, int count, float baseValue)
    {
        var ability = AbilityManager.Instance.GetAbility(abilityId);

        if (count == 0 || ability == null)
            return float.MaxValue;
        
        if (ability.AbilityType == AbilityApplyType.Rate)
        {
            for(int i = 0; i < count; i++)
            {
                baseValue *= ability.UpgradeAmount;
            }
        }
        else if (ability.AbilityType == AbilityApplyType.Fixed)
        {
            for(int i = 0; i < count; i++)
            {
                baseValue += ability.UpgradeAmount;
            }
        }
        
        return baseValue;
    }
    
    public void OnCloseInfoClicked()
    {
        gameObject.SetActive(false);
    }

    private void SetText(TextMeshProUGUI tmp, string value, float buffedValue = float.MaxValue, string suffix = "")
    {
        if (tmp == null) return;

        var sb = new System.Text.StringBuilder();

        if(value != null && buffedValue != float.MaxValue)
        {
            sb.Append($"{buffedValue}");
            sb.Append(suffix);
        }
        else
        {
            sb.Append(value ?? "-");
        }
        tmp.text = sb.ToString();
    }

    private void SetAllText(string value)
    {
        //left panel_tower
        SetText(towerIdValueText, value);
        // SetText(rangeTypeValueText, value);
        SetText(rangeValueText, value);
        // SetText(priorityTypeValueText, value);
        // SetText(priorityOrderValueText, value);
        SetText(fireRateValueText, value);
        SetText(hitRateValueText, value);
        SetText(spreadAccuracyValueText, value);
        //right panel_projectile
        // SetText(projectileTypeValueText, value);
        // SetText(projectilePrefabValueText, value);
        // SetText(hitEffectValueText, value);
        SetText(damageValueText, value);
        SetText(fixedPenetrationValueText, value);
        SetText(percentPenetrationValueText, value);
        // SetText(speedValueText, value);
        // SetText(accelerationValueText, value);
        SetText(targetNumberValueText, value);
        SetText(projectileNumberValueText, value);
        SetText(lifeTimeValueText, value);
        SetText(projectileSizeValueText, value);

        //left panel_tower
        towerIdValueText.color = Color.black;
        // rangeTypeValueText.color = Color.black;
        rangeValueText.color = Color.black;
        // priorityTypeValueText.color = Color.black;
        // priorityOrderValueText.color = Color.black;
        fireRateValueText.color = Color.black;
        hitRateValueText.color = Color.black;
        spreadAccuracyValueText.color = Color.black;

        //right panel_projectile
        // projectileTypeValueText.color = Color.black;
        // projectilePrefabValueText.color = Color.black;
        // hitEffectValueText.color = Color.black;
        damageValueText.color = Color.black;
        fixedPenetrationValueText.color = Color.black;
        percentPenetrationValueText.color = Color.black;
        // speedValueText.color = Color.black;
        // accelerationValueText.color = Color.black;
        targetNumberValueText.color = Color.black;
        projectileNumberValueText.color = Color.black;
        lifeTimeValueText.color = Color.black;
        projectileSizeValueText.color = Color.black;
    }
}
