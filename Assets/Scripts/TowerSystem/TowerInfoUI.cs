using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerInfoUI : PopUpUI
{
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private TextMeshProUGUI nameText;  //no use

    [Header("Left Panel - Labels")]
    [SerializeField] private TextMeshProUGUI towerIdLabelText;
    [SerializeField] private TextMeshProUGUI rangeLabelText;
    [SerializeField] private TextMeshProUGUI fireRateLabelText;
    [SerializeField] private TextMeshProUGUI hitRateLabelText;
    [SerializeField] private TextMeshProUGUI spreadLabelText;

    [Header("Right Panel - Labels")]
    [SerializeField] private TextMeshProUGUI damageLabelText;
    [SerializeField] private TextMeshProUGUI fixedPenLabelText;
    [SerializeField] private TextMeshProUGUI percentPenLabelText;
    [SerializeField] private TextMeshProUGUI targetNumLabelText;
    [SerializeField] private TextMeshProUGUI projectileNumLabelText;
    [SerializeField] private TextMeshProUGUI lifeTimeLabelText;
    [SerializeField] private TextMeshProUGUI projectileSizeLabelText;


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
            SetText(abilityExplainText, "no tower");
            return;
        }

        var attackTower = installControl.GetAttackTower(index); //Attack Tower Data
        var amplifierTower = installControl.GetAmplifierTower(index);

        if (attackTower != null && attackTower.AttackTowerData != null)
        {
            FillAttackTowerInfo(index, attackTower);
            SetAbilityExplainForAttack(attackTower);
            return;
        }

        if (amplifierTower != null && amplifierTower.AmplifierTowerData != null)
        {
            FillAmplifierTowerInfo(index, amplifierTower);
            SetAbilityExplainForAmplifier(amplifierTower);
            return;
        }

        if (nameText != null)
            nameText.text = $"Empty Slot {index}";
        SetAllText("-");
        SetText(abilityExplainText, "no tower");
    }

    protected override void Update()
    {
        touchPos = TouchManager.Instance.TouchPos;
        if (infoIndex == -1)
            return;

        if (RectTransformUtility.RectangleContainsScreenPoint(
               installControl.Towers[infoIndex].GetComponent<RectTransform>(),
               touchPos))
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

    private void FillAttackTowerInfo(int index, TowerAttack attackTower)
    {
        SetupAttackLabels();

        var attackTowerData = attackTower.AttackTowerData;
        var buffedProjectile = attackTower.CurrentProjectileData;

        if (nameText != null)
            nameText.text = $"{attackTowerData.towerId}";

        isSameTower = (infoIndex == index);

        var abilities = attackTower.Abilities;

        // Left panel : Tower Data
        SetText(towerIdValueText, attackTowerData.towerId);
        SetText(rangeValueText,
            attackTowerData.rangeData != null
                ? attackTowerData.rangeData.GetRange().ToString("0.0")
                : null);
        SetText(hitRateValueText, attackTowerData.hitRate.ToString("0.00") + "%");
        SetText(spreadAccuracyValueText, attackTowerData.spreadAccuracy.ToString("0.00") + "%");

        // Fire Rate
        SetText(
            fireRateValueText,
            $"{attackTower.AttackTowerData.fireRate:0.00}",
            attackTower.AttackTowerData.fireRate == attackTower.CurrentFireRate
                ? float.MaxValue
                : attackTower.CurrentFireRate
        );

        // Right panel
        var proj = attackTowerData.projectileType;

        if (proj != null)
        {
            SetText(
                damageValueText,
                proj.Attack.ToString("0.00"),
                CalculateEachAbility(200001, abilities, proj.Attack)
            );

            SetText(
                fixedPenetrationValueText,
                proj.FixedPenetration.ToString("0.00"),
                CalculateEachAbility(200004, abilities, proj.FixedPenetration)
            );

            SetText(
                percentPenetrationValueText,
                proj.RatePenetration.ToString("0.00") + "%",
                CalculateEachAbility(200003, abilities, proj.RatePenetration),
                "%"
            );

            SetText(projectileNumberValueText, "0"); 
            SetText(targetNumberValueText, proj.TargetNum.ToString());
            SetText(lifeTimeValueText, proj.RemainTime.ToString("0.00"));
            SetText(
                projectileSizeValueText,
                proj.CollisionSize.ToString("0.00"),
                CalculateEachAbility(200006, abilities, proj.CollisionSize)
            );
        }
        else
        {
            SetText(damageValueText, "-");
            SetText(fixedPenetrationValueText, "-");
            SetText(percentPenetrationValueText, "-");
            SetText(projectileNumberValueText, "-");
            SetText(targetNumberValueText, "-");
            SetText(lifeTimeValueText, "-");
            SetText(projectileSizeValueText, "-");
        }
    }
    private void SetAbilityExplainForAttack(TowerAttack attackTower)
    {
        var abilities = attackTower.Abilities;
        if (abilities == null || abilities.Count == 0)
        {
            SetText(abilityExplainText, "no ability");
            return;
        }

        var ability = AbilityManager.Instance.GetAbility(abilities[0]);
        SetText(abilityExplainText, ability?.ToString() ?? "no ability");
    }

    private void FillAmplifierTowerInfo(int index, TowerAmplifier amplifierTower)
    {
        SetupAmplifierLabels();
        SetText(towerIdValueText, "-");

        var ampData = amplifierTower.AmplifierTowerData;
        var slots = amplifierTower.BuffedSlotIndex;

        if (ampData == null)
        {
            if (nameText != null)
                nameText.text = $"Amplifier {index}";
            SetAllText("no data");
            return;
        }

        if (nameText != null)
            nameText.text = ampData.AmplifierId;

        SetText(rangeValueText, ampData.AmplifierType.ToString());
        SetText(fireRateValueText, ampData.TargetMode.ToString());
        SetText(hitRateValueText, ampData.FixedBuffedSlotCount.ToString());
        SetText(spreadAccuracyValueText, ampData.OnlyAttackTower ? "Attack Only" : "All Towers");

        SetText(damageValueText, $"x{ampData.DamageBuff:0.00}");         
        SetText(fixedPenetrationValueText, $"x{ampData.FireRateBuff:0.00}");
        SetText(percentPenetrationValueText, $"+{ampData.ProjectileCountBuff}"); 
        SetText(targetNumberValueText, $"+{ampData.TargetNumberBuff}");   
        SetText(projectileNumberValueText, $"+{ampData.HitRadiusBuff:0.00}"); 
        SetText(lifeTimeValueText, $"x{ampData.PercentPenetrationBuff:0.00}");  
        SetText(projectileSizeValueText, $"+{ampData.FixedPenetrationBuff:0.00}");
    }

    private void SetAbilityExplainForAmplifier(TowerAmplifier amplifierTower)
    {
        if (abilityExplainText == null) return;

        var ampData = amplifierTower.AmplifierTowerData;
        var slots = amplifierTower.BuffedSlotIndex;

        if (ampData == null)
        {
            abilityExplainText.text = "no buff";
            return;
        }

        var sb = new StringBuilder();

        if (slots != null && slots.Count > 0)
        {
            sb.AppendLine($"버프 슬롯: {string.Join(", ", slots)}");
        }
        else
        {
            sb.AppendLine("버프 슬롯: 없음");
        }

        var buffParts = new List<string>();

        if (ampData.DamageBuff != 1f)
            buffParts.Add($"공격력 x{ampData.DamageBuff:0.00}");
        if (ampData.FireRateBuff != 1f)
            buffParts.Add($"공속 x{ampData.FireRateBuff:0.00}");
        if (ampData.ProjectileCountBuff != 0)
            buffParts.Add($"투사체 +{ampData.ProjectileCountBuff}");
        if (ampData.TargetNumberBuff != 0)
            buffParts.Add($"타겟수 +{ampData.TargetNumberBuff}");
        if (ampData.HitRadiusBuff != 0f)
            buffParts.Add($"히트 반경 +{ampData.HitRadiusBuff:0.00}");
        if (ampData.PercentPenetrationBuff != 1f)
            buffParts.Add($"관통률 x{ampData.PercentPenetrationBuff:0.00}");
        if (ampData.FixedPenetrationBuff != 0f)
            buffParts.Add($"고정 관통 +{ampData.FixedPenetrationBuff:0.00}");
        if (ampData.HitRateBuff != 1f)
            buffParts.Add($"명중률 x{ampData.HitRateBuff:0.00}");

        if (buffParts.Count > 0)
        {
            sb.Append(string.Join(", ", buffParts));
        }
        else
        {
            sb.Append("추가 버프 없음");
        }

        abilityExplainText.text = sb.ToString();
    }

    private void SetupAttackLabels()
    {
        if (towerIdLabelText != null) towerIdLabelText.text = "타워 ID";
        if (rangeLabelText != null) rangeLabelText.text = "사거리";
        if (fireRateLabelText != null) fireRateLabelText.text = "공격 속도";
        if (hitRateLabelText != null) hitRateLabelText.text = "명중률";
        if (spreadLabelText != null) spreadLabelText.text = "정확도";

        if (damageLabelText != null) damageLabelText.text = "공격력";
        if (fixedPenLabelText != null) fixedPenLabelText.text = "고정 관통";
        if (percentPenLabelText != null) percentPenLabelText.text = "관통률";
        if (targetNumLabelText != null) targetNumLabelText.text = "타겟 수";
        if (projectileNumLabelText != null) projectileNumLabelText.text = "투사체 수";
        if (lifeTimeLabelText != null) lifeTimeLabelText.text = "수명";
        if (projectileSizeLabelText != null) projectileSizeLabelText.text = "hitbox 크기";
    }

    private void SetupAmplifierLabels()
    {
        if (towerIdLabelText != null) towerIdLabelText.text = "-";
        if (rangeLabelText != null) rangeLabelText.text = "증폭 타입";
        if (fireRateLabelText != null) fireRateLabelText.text = "증폭 ID";
        if (hitRateLabelText != null) hitRateLabelText.text = "버프 슬롯 수";
        if (spreadLabelText != null) spreadLabelText.text = "buff";

        if (damageLabelText != null) damageLabelText.text = "공격력+";
        if (fixedPenLabelText != null) fixedPenLabelText.text = "공속 배율";
        if (percentPenLabelText != null) percentPenLabelText.text = "투사체 수 +";
        if (targetNumLabelText != null) targetNumLabelText.text = "타겟 수 +";
        if (projectileNumLabelText != null) projectileNumLabelText.text = "hit 반경 +";
        if (lifeTimeLabelText != null) lifeTimeLabelText.text = "관통률 배율";
        if (projectileSizeLabelText != null) projectileSizeLabelText.text = "고정 관통 +";
    }

}
