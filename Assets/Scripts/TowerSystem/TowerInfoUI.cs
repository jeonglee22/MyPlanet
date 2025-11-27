using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private Button openAbilityInfoButton;
    [SerializeField] private GameObject abilityInfoPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject abilityExplainContent;
    private RectTransform contentRect;

    private int infoIndex = -1;
    private bool isSameTower;

    private void Start()
    {
        abilityInfoPanel.SetActive(false);
        openAbilityInfoButton.onClick.AddListener(() => abilityInfoPanel.SetActive(!abilityInfoPanel.activeSelf));
    }

    private void OnEnable()
    {
        contentRect = scrollRect?.content;
        abilityInfoPanel.SetActive(false);
    }

    public void SetInfo(int index)
    {
        Debug.Log($"[TowerInfoUI.SetInfo] index={index}, installControl={(installControl != null)}");

        nameText.text = $"Tower {index}";
        contentRect.DetachChildren();

        if (installControl == null)
        {
            nameText.text = "No data";
            SetAllText(null);
            
            var textNull = Instantiate(abilityExplainContent, contentRect);
            SetText(textNull.GetComponent<TextMeshProUGUI>(), "no tower");
            return;
        }

        infoIndex = index;
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

        if (nameText != null) nameText.text = $"Empty Slot {index}";
        SetAllText("-");
        
        var textEmpty = Instantiate(abilityExplainContent, contentRect);
        SetText(textEmpty.GetComponent<TextMeshProUGUI>(), "no tower");
    }

    protected override void Update()
    {
        touchPos = TouchManager.Instance.TouchPos;
        
        if (infoIndex == -1) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(
               installControl.Towers[infoIndex].GetComponent<RectTransform>(),
               touchPos))
        {
            return;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(
               scrollRect.gameObject.GetComponent<RectTransform>(),
               touchPos))
        {
            return;
        }

        base.Update();
    }

    private float CalculateEachAbility(int abilityId, List<int> abilities, float baseValue)
    {
        if (abilities == null || abilities.Count == 0) return baseValue;

        int count = abilities.FindAll(x => x == abilityId).Count;
        return CalculateAbilityUpgradeValue(abilityId, count, baseValue);
    }

    private float CalculateAbilityUpgradeValue(int abilityId, int count, float baseValue)
    {
        var ability = AbilityManager.GetAbility(abilityId);

        if (count == 0 || ability == null) return baseValue;

        float result = baseValue;

        if (ability.AbilityType == AbilityApplyType.Rate)
        {
            for (int i = 0; i < count; i++)
            {
                result *= ability.UpgradeAmount;
            }
        }
        else if (ability.AbilityType == AbilityApplyType.Fixed)
        {
            for (int i = 0; i < count; i++)
            {
                result += ability.UpgradeAmount;
            }
        }

        return result;
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

    private void SetStatText(TextMeshProUGUI tmp, float baseValue, float finalValue, string format = "0.00", string suffix = "")
    {
        if (tmp == null) return;

        bool hasBase = !Mathf.Approximately(baseValue, 0f);
        bool hasFinal = !Mathf.Approximately(finalValue, 0f);

        if (!hasBase && !hasFinal)
        {
            tmp.text = "-";
            return;
        }

        if (Mathf.Approximately(baseValue, finalValue))
        {
            tmp.text = $"{baseValue.ToString(format)}{suffix}";
            return;
        }

        tmp.text = $"{finalValue.ToString(format)}{suffix}";
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
    }

    private void FillAttackTowerInfo(int index, TowerAttack attackTower)
    {
        SetupAttackLabels();

        var attackTowerData = attackTower.AttackTowerData;
        int level = attackTower.ReinforceLevel;

        if (nameText != null)
        {
            nameText.text = $"{attackTowerData.towerId} (Lv.{level})";
        }

        isSameTower = (infoIndex == index);
        var abilities = attackTower.Abilities;

        SetText(
            towerIdValueText,
            $"{attackTowerData.towerId} (Lv.{level})"
        );

        //Range
        SetText(rangeValueText,
            attackTowerData.rangeData != null
                ? attackTowerData.rangeData.GetRange().ToString("0.0")
                : null);

        //FireRate
        float baseFireRate = attackTower.BasicFireRate;
        float finalFireRate = attackTower.CurrentFireRate;
        SetStatText(fireRateValueText, baseFireRate, finalFireRate, "0.00");

        //Hit Rate
        float baseHitRate = attackTowerData.Accuracy;
        float finalHitRate = attackTower.FinalHitRate;
        SetStatText(hitRateValueText, baseHitRate, finalHitRate, "0.00", "%");

        //Spread Accuracy
        if (spreadAccuracyValueText != null)
            spreadAccuracyValueText.text = attackTowerData.grouping.ToString("0.00") + "%";
        //----------------------------------------------------------

        // Right panel----------------------------------------------
        var baseProj = attackTower.BaseProjectileData ?? attackTowerData.projectileType;
        var buffedProj = attackTower.BuffedProjectileData ?? baseProj;

        if (baseProj != null)
        {
            // Attack(base + ability + amp)
            float baseDamage = baseProj.Attack;
            float ampDamage = buffedProj.Attack;
            float finalDamage = CalculateEachAbility((int)AbilityId.AttackDamage, abilities, ampDamage);
            SetStatText(damageValueText, baseDamage, finalDamage, "0.00");

            // Fixed Penetration
            float baseFixedPen = baseProj.FixedPenetration;
            float ampFixedPen = buffedProj.FixedPenetration;
            float finalFixedPen = CalculateEachAbility((int)AbilityId.FixedPanetration, abilities, ampFixedPen);
            SetStatText(fixedPenetrationValueText, baseFixedPen, finalFixedPen, "0.00");

            // Percent Penetration
            float baseRatePen = baseProj.RatePenetration;
            float ampRatePen = buffedProj.RatePenetration;
            float finalRatePen = CalculateEachAbility((int)AbilityId.PercentPenetration, abilities, ampRatePen);
            SetStatText(percentPenetrationValueText, baseRatePen, finalRatePen, "0.00", "%");

            // Proejctile Count
            float baseCount = attackTower.BaseProjectileCount;
            float finalCount = attackTower.CurrentProjectileCount;
            SetStatText(projectileNumberValueText, baseCount, finalCount, "0");

            // Target Num
            float baseTargets = baseProj.TargetNum;
            float ampTargets = buffedProj.TargetNum;
            float finalTargets = CalculateEachAbility(
                (int)AbilityId.TargetCount, 
                abilities, 
                ampTargets);
            SetStatText(targetNumberValueText, baseTargets, finalTargets, "0");

            // LifeTime
            float baseLifeTime = baseProj.RemainTime;
            float finalLifeTime = buffedProj.RemainTime;
            SetStatText(lifeTimeValueText, baseLifeTime, finalLifeTime, "0.00");

            // Hitbox Size
            float baseSize = baseProj.CollisionSize;
            float ampSize = buffedProj.CollisionSize;
            float finalSize = CalculateEachAbility((int)AbilityId.CollisionSize, abilities, ampSize);
            Debug.Log($"[TowerInfoUI] size slot={index}, base={baseSize}, buffed={ampSize}, final={finalSize}");
            SetStatText(projectileSizeValueText, baseSize, finalSize, "0.00");
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
            var text = Instantiate(abilityExplainContent, contentRect);
            SetText(text.GetComponent<TextMeshProUGUI>(), "no ability");
            return;
        }

        foreach (var abilityId in abilities)
        {
            var ability = AbilityManager.GetAbility(abilityId);
            var text = Instantiate(abilityExplainContent, contentRect);
            SetText(text.GetComponent<TextMeshProUGUI>(), ability?.ToString() ?? "no ability");
        }
    }

    private void FillAmplifierTowerInfo(int index, TowerAmplifier amplifierTower)
    {
        SetupAmplifierLabels();

        var ampData = amplifierTower.AmplifierTowerData;
        var slots = amplifierTower.BuffedSlotIndex;

        if (ampData == null)
        {
            if (nameText != null) nameText.text = $"Amplifier {index}";
            SetAllText("no data");
            return;
        }

        string baseName = !string.IsNullOrEmpty(ampData.BuffTowerName)
            ? ampData.BuffTowerName
            : $"Amplifier {index}";

        int level = amplifierTower.ReinforceLevel;

        if (nameText != null)
            nameText.text = $"{baseName} (Lv.{level})";

        SetText(
            towerIdValueText,
            $"{baseName} (Lv.{level})"
        );

        // 왼쪽 패널: 이름 / 타입 / 슬롯 수 / 타겟 종류
        SetText(rangeValueText,
            !string.IsNullOrEmpty(ampData.BuffTowerName)
                ? ampData.BuffTowerName
                : ampData.AmplifierType.ToString());

        SetText(fireRateValueText, ampData.AmplifierType.ToString());
        SetText(hitRateValueText, ampData.FixedBuffedSlotCount.ToString());
        SetText(spreadAccuracyValueText,
            ampData.OnlyAttackTower ? "공격 타워만" : "모든 타워");

        // 오른쪽 패널: 수치들 (퍼센트/추가값 포맷 맞추기)

        // 공격력% (DamageBuff: add, 0.4 -> +40%)
        string dmgText = FormatPercentFromAdd(ampData.DamageBuff);
        SetText(damageValueText, dmgText ?? "-");

        // 공속% (FireRateBuff: mul, 1.2 -> +20%)
        string fireRateText = FormatPercentFromMul(ampData.FireRateBuff);
        SetText(fixedPenetrationValueText, fireRateText ?? "-");

        // 투사체 수 +N
        string projCountText = ampData.ProjectileCountBuff != 0
            ? $"{ampData.ProjectileCountBuff:+0;-0}"
            : "-";
        SetText(percentPenetrationValueText, projCountText);

        // 타겟 수 +N
        string targetNumText = ampData.TargetNumberBuff != 0
            ? $"{ampData.TargetNumberBuff:+0;-0}"
            : "-";
        SetText(targetNumberValueText, targetNumText);

        // 히트 반경% (HitRadiusBuff: add, 0.25 -> +25%)
        string hitRadiusText = FormatPercentFromMul(ampData.HitRadiusBuff);
        SetText(projectileNumberValueText, hitRadiusText ?? "-");

        // 비율 관통력% (PercentPenetrationBuff: mul, 1.5 -> +50%)
        string ratePenText = FormatPercentFromMul(ampData.PercentPenetrationBuff);
        SetText(lifeTimeValueText, ratePenText ?? "-");

        // 고정 관통 +N
        string fixedPenText = !Mathf.Approximately(ampData.FixedPenetrationBuff, 0f)
            ? $"{ampData.FixedPenetrationBuff:+0.##;-0.##}"
            : "-";
        SetText(projectileSizeValueText, fixedPenText);
    }

    private void SetAbilityExplainForAmplifier(TowerAmplifier amplifierTower)
    {
        if (abilityExplainText == null) return;

        var ampData = amplifierTower.AmplifierTowerData;
        var slots = amplifierTower.BuffedSlotIndex;

        if (ampData == null)
        {
            var textEmpty = Instantiate(abilityExplainContent, contentRect);
            SetText(textEmpty.GetComponent<TextMeshProUGUI>(), "no buff");
            return;
        }

        var sb = new StringBuilder();

        // ── 기본 정보 ───────────────────────────────
        if (!string.IsNullOrEmpty(ampData.BuffTowerName))
            sb.AppendLine($"이름: {ampData.BuffTowerName}");

        if (slots != null && slots.Count > 0)
            sb.AppendLine($"버프 슬롯: {string.Join(", ", slots)}");
        else
            sb.AppendLine("버프 슬롯: 없음");

        // ── 랜덤능력 정보 ───────────────────────────
        var ampAbilities = amplifierTower.Abilities;
        if (ampAbilities != null && ampAbilities.Count > 0)
        {
            int randAbilityId = ampAbilities[0];
            var raRow = DataTableManager.RandomAbilityTable.Get(randAbilityId);
            if (raRow != null)
            {
                sb.AppendLine();
                sb.AppendLine($"랜덤 능력: {raRow.RandomAbilityName} (ID: {randAbilityId})");

                // ⚠️ 여기서부터는 RandomAbility 테이블에
                // PlaceType, AddSlotNum, DuplicateType 프로퍼티가 있다고 가정하고 쓴다.
                int placeType = raRow.PlaceType;
                int addSlotNum = raRow.AddSlotNum;
                int duplicateType = raRow.DuplicateType;

                string placeDesc = placeType switch
                {
                    0 => "배치타입 0: 증폭 버프 슬롯과 별도 슬롯에 랜덤능력 배치",
                    1 => "배치타입 1: 기존 증폭 버프 슬롯 중 하나에 랜덤능력 집중",
                    2 => $"배치타입 2: 기본 버프 슬롯 수가 {addSlotNum}개 증가",
                    _ => $"배치타입 {placeType}"
                };

                sb.AppendLine(placeDesc);
                sb.AppendLine($"중복 타입: {(duplicateType == 0 ? "중첩 가능" : "중첩 불가")}");
            }
        }

        // ── 증폭 수치 설명 (기존 코드 기반) ───────────
        var buffParts = new List<string>();

        if (!Mathf.Approximately(ampData.DamageBuff, 0f))
            buffParts.Add($"공격력 {FormatPercentFromAdd(ampData.DamageBuff)}");
        if (!Mathf.Approximately(ampData.FireRateBuff, 1f))
            buffParts.Add($"공속 {FormatPercentFromMul(ampData.FireRateBuff)}");
        if (ampData.ProjectileCountBuff != 0)
            buffParts.Add($"투사체 {ampData.ProjectileCountBuff:+0;-0}");
        if (ampData.TargetNumberBuff != 0)
            buffParts.Add($"타겟 수 {ampData.TargetNumberBuff:+0;-0}");
        if (!Mathf.Approximately(ampData.HitRadiusBuff, 1f))
            buffParts.Add($"히트 반경 {FormatPercentFromMul(ampData.HitRadiusBuff)}");
        if (!Mathf.Approximately(ampData.PercentPenetrationBuff, 1f))
            buffParts.Add($"관통률 {FormatPercentFromMul(ampData.PercentPenetrationBuff)}");
        if (!Mathf.Approximately(ampData.FixedPenetrationBuff, 0f))
            buffParts.Add($"고정 관통 {ampData.FixedPenetrationBuff:+0.##;-0.##}");
        if (!Mathf.Approximately(ampData.HitRateBuff, 1f))
            buffParts.Add($"명중률 {FormatPercentFromMul(ampData.HitRateBuff)}");

        if (buffParts.Count > 0)
        {
            sb.AppendLine();
            sb.Append(string.Join(", ", buffParts));
        }
        else
        {
            sb.AppendLine();
            sb.Append("추가 버프 없음");
        }
        var text = Instantiate(abilityExplainContent, contentRect);
        SetText(text.GetComponent<TextMeshProUGUI>(), sb.ToString());
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
        if (rangeLabelText != null) rangeLabelText.text = "Buff Tower Name";
        if (fireRateLabelText != null) fireRateLabelText.text = "Buff Type";
        if (hitRateLabelText != null) hitRateLabelText.text = "버프 슬롯 수";
        if (spreadLabelText != null) spreadLabelText.text = "Buff Target";

        if (damageLabelText != null) damageLabelText.text = "공격력+";
        if (fixedPenLabelText != null) fixedPenLabelText.text = "공속 배율";
        if (percentPenLabelText != null) percentPenLabelText.text = "투사체 수 +";
        if (targetNumLabelText != null) targetNumLabelText.text = "타겟 수 +";
        if (projectileNumLabelText != null) projectileNumLabelText.text = "hit 반경 +";
        if (lifeTimeLabelText != null) lifeTimeLabelText.text = "관통률 배율";
        if (projectileSizeLabelText != null) projectileSizeLabelText.text = "고정 관통 +";
    }

    // 0.4 -> "+40%"
    private string FormatPercentFromAdd(float add)
    {
        if (Mathf.Approximately(add, 0f)) return null;
        float p = add * 100f;
        return $"{p:+0.##;-0.##}%";
    }

    // 1.2 -> "+20%"
    private string FormatPercentFromMul(float mul)
    {
        if (Mathf.Approximately(mul, 1f)) return null;
        float p = (mul - 1f) * 100f;
        return $"{p:+0.##;-0.##}%";
    }
}