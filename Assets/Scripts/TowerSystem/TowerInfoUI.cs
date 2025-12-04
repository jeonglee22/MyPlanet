using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerInfoUI : MonoBehaviour
{
    [SerializeField] private TowerInstallControl installControl;
    
    //TowerNameInfo
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Switch Data Panel")]
    [SerializeField] private GameObject attackTowerDataPanel;
    [SerializeField] private GameObject buffTowerDataPanel;

    [Header("Buff Tower Text")]
    [SerializeField] private TextMeshProUGUI buffSlotInfoText;  
    [SerializeField] private TextMeshProUGUI randomSlotInfoText;
    [SerializeField] private RectTransform basicEffectListRoot; 
    [SerializeField] private RectTransform randomEffectListRoot;
    [SerializeField] private GameObject effectLinePrefab;

    [Header("Attack Tower Basic Data")]
    [SerializeField] private TextMeshProUGUI damageValueText;
    [SerializeField] private TextMeshProUGUI fireRateValueText;
    [SerializeField] private TextMeshProUGUI fixedPenetrationValueText;
    [SerializeField] private TextMeshProUGUI percentPenetrationValueText;
    [SerializeField] private TextMeshProUGUI hitRateValueText;
    [SerializeField] private TextMeshProUGUI spreadAccuracyValueText;

    //ADD VALUE
    [SerializeField] private TextMeshProUGUI targetNumberValueText;
    [SerializeField] private TextMeshProUGUI projectileNumberValueText;
    [SerializeField] private TextMeshProUGUI lifeTimeValueText;
    [SerializeField] private TextMeshProUGUI projectileSizeValueText;

    private TextMeshProUGUI rangeValueText;
    private TextMeshProUGUI towerIdValueText;

    [Header("Ability Panel")]
    [SerializeField] private TextMeshProUGUI abilityExplainText;
    [SerializeField] private Button openAbilityInfoButton;
    [SerializeField] private GameObject abilityInfoPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject abilityExplainContent;
    private RectTransform contentRect;

    private int infoIndex = -1;
    public int CurrentSlotIndex => infoIndex;
    private bool isSameTower;

    private void Start()
    {
        abilityInfoPanel.SetActive(false);
        openAbilityInfoButton.onClick.AddListener(
            () => abilityInfoPanel.SetActive(!abilityInfoPanel.activeSelf));
    }

    private void OnEnable()
    {
        contentRect = scrollRect?.content;
        abilityInfoPanel.SetActive(false);

        if (attackTowerDataPanel != null) attackTowerDataPanel.SetActive(false);
        if (buffTowerDataPanel != null) buffTowerDataPanel.SetActive(false);

        if (installControl == null) return;

        foreach (var amp in installControl.GetAllAmplifiers())
        {
            if (amp == null) continue;
            amp.OnBuffTargetsChanged -= HandleBuffChanged; 
            amp.OnBuffTargetsChanged += HandleBuffChanged;
        }
    }

    private void OnDisable()
    {
        if (installControl == null) return;

        foreach (var amp in installControl.GetAllAmplifiers())
        {
            if (amp == null) continue;
            amp.OnBuffTargetsChanged -= HandleBuffChanged;
        }
    }

    private void HandleBuffChanged()
    {
        if (gameObject.activeSelf && CurrentSlotIndex >= 0)
        {
            SetInfo(CurrentSlotIndex);
        }
    }

    public void SetInfo(int index)
    {
        if (contentRect == null && scrollRect != null)
            contentRect = scrollRect.content;

        if (contentRect != null)
            contentRect.DetachChildren();

        if (installControl == null)
        {
            nameText.text = "No data";
            SetAllText(null);

            if (attackTowerDataPanel != null) attackTowerDataPanel.SetActive(false);
            if (buffTowerDataPanel != null) buffTowerDataPanel.SetActive(false);

            var textNull = Instantiate(abilityExplainContent, contentRect);
            SetText(textNull.GetComponent<TextMeshProUGUI>(), "no tower");
            return;
        }

        infoIndex = index;
        var attackTower = installControl.GetAttackTower(index); //Attack Tower Data
        var amplifierTower = installControl.GetAmplifierTower(index);

        if (attackTower != null && attackTower.AttackTowerData != null)
        {
            if (attackTowerDataPanel != null) attackTowerDataPanel.SetActive(true);
            if (buffTowerDataPanel != null) buffTowerDataPanel.SetActive(false);

            FillAttackTowerInfo(index, attackTower);
            SetAbilityExplainForAttack(attackTower);
            return;
        }

        if (amplifierTower != null && amplifierTower.AmplifierTowerData != null)
        {
            if (attackTowerDataPanel != null) attackTowerDataPanel.SetActive(false);
            if (buffTowerDataPanel != null) buffTowerDataPanel.SetActive(true);

            FillAmplifierTowerInfo(index, amplifierTower);
            SetAbilityExplainForAmplifier(amplifierTower);
            return;
        }

        if (nameText != null) nameText.text = $"Empty Slot {index}";
        SetAllText("-");

        if (attackTowerDataPanel != null) attackTowerDataPanel.SetActive(false);
        if (buffTowerDataPanel != null) buffTowerDataPanel.SetActive(false);

        var textEmpty = Instantiate(abilityExplainContent, contentRect);
        SetText(textEmpty.GetComponent<TextMeshProUGUI>(), "no tower");
    }

    protected void Update()
    {
        var touchPos = TouchManager.Instance.TouchPos;
        
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
        if (installControl != null)
        {
            var method = installControl.GetType().GetMethod("ClearAllSlotHighlights",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(installControl, null);
            }
        }

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
            tmp.text = $"0{suffix}";
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
        SetText(damageValueText, value);
        SetText(fireRateValueText, value);
        SetText(fixedPenetrationValueText, value);
        SetText(percentPenetrationValueText, value);
        SetText(hitRateValueText, value);
        SetText(spreadAccuracyValueText, value);

        SetText(targetNumberValueText, value);
        SetText(projectileNumberValueText, value);

        SetText(lifeTimeValueText, value);
        SetText(projectileSizeValueText, value);

        SetText(towerIdValueText, value);
        SetText(rangeValueText, value);
    }

    private void FillAttackTowerInfo(int index, TowerAttack attackTower)
    {
        var attackTowerData = attackTower.AttackTowerData;
        int level = attackTower.ReinforceLevel;

        if (nameText != null) nameText.text = $"{attackTowerData.towerId} (Lv.{level})";

        isSameTower = (infoIndex == index);
        var abilities = attackTower.Abilities;

        SetText(towerIdValueText, $"{attackTowerData.towerId} (Lv.{level})");

        var baseProj = attackTower.BaseProjectileData ?? attackTowerData.projectileType;
        var buffedProj = attackTower.BuffedProjectileData ?? baseProj;

        if (baseProj != null)
        {
            // Attack(base + ability + amp)
            float baseDamage = baseProj.Attack;
            float finalDamage = buffedProj.Attack;
            SetStatText(damageValueText, baseDamage, finalDamage, "0.00");
            // Fixed Penetration
            float baseFixedPen = baseProj.FixedPenetration;
            float finalFixedPen = buffedProj.FixedPenetration;
            SetStatText(fixedPenetrationValueText, baseFixedPen, finalFixedPen, "0.00");
            // Percent Penetration
            float baseRatePen = baseProj.RatePenetration;
            float finalRatePen = buffedProj.RatePenetration;
            SetStatText(percentPenetrationValueText, baseRatePen, finalRatePen, "0.00", "%");
            // Projectile Count
            float baseCount = attackTower.BaseProjectileCount;
            float finalCount = attackTower.CurrentProjectileCount;
            SetStatText(projectileNumberValueText, baseCount, finalCount, "0");
            // Target Num
            float baseTargets = baseProj.TargetNum;
            float finalTargets = buffedProj.TargetNum;
            SetStatText(targetNumberValueText, baseTargets, finalTargets, "0");
            // LifeTime
            float baseLifeTime = baseProj.RemainTime;
            float finalLifeTime = buffedProj.RemainTime;
            SetStatText(lifeTimeValueText, baseLifeTime, finalLifeTime, "0.00");
            // Hitbox Size
            float baseSize = baseProj.CollisionSize;
            float finalSize = buffedProj.CollisionSize;
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
        //FireRate
        float baseFireRate = attackTower.BasicFireRate;
        float finalFireRate = attackTower.CurrentFireRate;
        SetStatText(fireRateValueText, baseFireRate, finalFireRate, "0.00");

        //Hit Rate (명중률)
        float baseHitRate = attackTowerData.Accuracy;
        float finalHitRate = attackTower.FinalHitRate;
        SetStatText(hitRateValueText, baseHitRate, finalHitRate, "0.00", "%");

        //Spread Accuracy
        if (spreadAccuracyValueText != null)
            spreadAccuracyValueText.text = attackTowerData.grouping.ToString("0.00") + "%";

        //Range 
        SetText(rangeValueText, attackTowerData.rangeData != null
                ? attackTowerData.rangeData.GetRange().ToString("0.0") : null);
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
        var ampData = amplifierTower.AmplifierTowerData;
        var slots = amplifierTower.BuffedSlotIndex;

        if (ampData == null)
        {
            if (nameText != null) nameText.text = $"Amplifier {index}";
            SetAllText("no data");

            if (buffSlotInfoText != null) buffSlotInfoText.text = "버프 슬롯 없음";
            if (randomSlotInfoText != null) randomSlotInfoText.text = "랜덤 슬롯 없음";
            return;
        }

        string baseName = !string.IsNullOrEmpty(ampData.BuffTowerName)
            ? ampData.BuffTowerName : $"Amplifier {index}";

        int level = amplifierTower.ReinforceLevel;

        //Name Object
        if (nameText != null) nameText.text = $"{baseName} (Lv.{level})";
        SetText(towerIdValueText, $"{baseName} (Lv.{level})");

        //Slot Index Info
        if (buffSlotInfoText != null)
        {
            int selfIndex = amplifierTower.SelfIndex;
            string buffBlock = FormatOffsetArray(amplifierTower.BuffedSlotIndex, selfIndex);
            buffSlotInfoText.text = buffBlock;
        }

        if (randomSlotInfoText != null)
        {
            string randomInfo = BuildRandomSlotInfo(amplifierTower);
            randomSlotInfoText.text = randomInfo;
        }

        ClearBuffEffectLists();
        FillBasicBuffEffects(amplifierTower);
        FillRandomAbilityEffects(amplifierTower);

        // Buff Panel--------------------------------
        SetText(rangeValueText,
            !string.IsNullOrEmpty(ampData.BuffTowerName)
                ? ampData.BuffTowerName
                : ampData.AmplifierType.ToString());

        SetText(fireRateValueText, ampData.AmplifierType.ToString());
        SetText(hitRateValueText, ampData.FixedBuffedSlotCount.ToString());
        SetText(spreadAccuracyValueText,
            ampData.OnlyAttackTower ? "공격 타워만" : "모든 타워");

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
        string hitRadiusText = FormatPercentFromAdd(ampData.HitRadiusBuff);
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

        if (!string.IsNullOrEmpty(ampData.BuffTowerName))
            sb.AppendLine($"이름: {ampData.BuffTowerName}");

        if (slots != null && slots.Count > 0)
            sb.AppendLine($"버프 슬롯: {string.Join(", ", slots)}");
        else
            sb.AppendLine("버프 슬롯: 없음");

        //random ability
        var ampAbilities = amplifierTower.Abilities;
        if (ampAbilities != null && ampAbilities.Count > 0)
        {
            int randAbilityId = ampAbilities[0];
            var raRow = DataTableManager.RandomAbilityTable.Get(randAbilityId);
            if (raRow != null)
            {
                sb.AppendLine();
                sb.AppendLine($"랜덤 능력: {raRow.RandomAbilityName} (ID: {randAbilityId})");

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

        //buff data
        var buffParts = new List<string>();

        if (!Mathf.Approximately(ampData.DamageBuff, 0f))
            buffParts.Add($"공격력 {FormatPercentFromAdd(ampData.DamageBuff)}");
        if (!Mathf.Approximately(ampData.FireRateBuff, 1f))
            buffParts.Add($"공속 {FormatPercentFromMul(ampData.FireRateBuff)}");
        if (ampData.ProjectileCountBuff != 0)
            buffParts.Add($"투사체 {ampData.ProjectileCountBuff:+0;-0}");
        if (ampData.TargetNumberBuff != 0)
            buffParts.Add($"타겟 수 {ampData.TargetNumberBuff:+0;-0}");
        if (!Mathf.Approximately(ampData.HitRadiusBuff, 0f))
            buffParts.Add($"히트 반경 {FormatPercentFromAdd(ampData.HitRadiusBuff)}");
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
    private string FormatOffsetArray(IReadOnlyList<int> targetSlots, int selfIndex)
    {
        // 카드 쪽이랑 맞추기: 슬롯이 없으면 빈 문자열
        if (targetSlots == null || targetSlots.Count == 0)
            return string.Empty;

        if (installControl == null)
            return string.Empty;

        int towerCount = installControl.TowerCount;
        if (towerCount <= 1)
            return string.Empty;

        // 오른쪽 / 왼쪽 상대 거리 리스트
        List<int> rightList = new List<int>();
        List<int> leftList = new List<int>();

        foreach (int slot in targetSlots)
        {
            if (slot < 0) continue;       // 안전장치
            if (slot == selfIndex) continue; // 자기 자신은 표시 안 함

            // 원형(시계 방향 / 반시계 방향) 기준 거리 계산
            int cw = (slot - selfIndex + towerCount) % towerCount;   // 시계 방향(오른쪽) 거리
            int ccw = (selfIndex - slot + towerCount) % towerCount;  // 반시계 방향(왼쪽) 거리

            // 둘 다 0이면 자기 자신이라 스킵(원래 위에서 걸러짐)
            if (cw == 0 && ccw == 0)
                continue;

            // 더 가까운 방향을 선택 (동일하면 오른쪽으로 처리)
            if (cw <= ccw)
            {
                if (cw > 0)
                    rightList.Add(cw);    // 오른쪽 n칸
            }
            else
            {
                if (ccw > 0)
                    leftList.Add(ccw);    // 왼쪽 n칸
            }
        }

        // 진짜 표시할 게 없으면 빈 문자열
        if (rightList.Count == 0 && leftList.Count == 0)
            return string.Empty;

        rightList.Sort();
        leftList.Sort();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("증폭타워 기준");

        if (rightList.Count > 0)
        {
            var rightPos = new List<string>();
            foreach (int v in rightList)
                rightPos.Add($"{v}번째");

            sb.AppendLine($"오른쪽 {string.Join(", ", rightPos)}");
        }

        if (leftList.Count > 0)
        {
            var leftPos = new List<string>();
            foreach (int v in leftList)
                leftPos.Add($"{v}번째");

            sb.AppendLine($"왼쪽 {string.Join(", ", leftPos)}");
        }

        return sb.ToString();
    }


    private string BuildRandomSlotInfo(TowerAmplifier amp)
    {
        if (amp == null) return "-";

        int selfIndex = amp.SelfIndex;
        var randomSlots = amp.RandomAbilitySlotIndex;

        // 랜덤 능력 이름
        string abilityName = null;
        var abilities = amp.Abilities;
        if (abilities != null && abilities.Count > 0)
        {
            int randAbilityId = abilities[0];
            var raRow = DataTableManager.RandomAbilityTable?.Get(randAbilityId);
            if (raRow != null)
                abilityName = raRow.RandomAbilityName;
        }

        // 슬롯 오프셋 문자열
        string randomBlock = FormatOffsetArray(randomSlots, selfIndex);

        // 둘 다 없으면
        if (string.IsNullOrEmpty(abilityName) && string.IsNullOrEmpty(randomBlock))
            return "랜덤 슬롯 없음";

        // 능력 이름만 있는 경우
        if (!string.IsNullOrEmpty(abilityName) && string.IsNullOrEmpty(randomBlock))
            return abilityName;

        // 슬롯 정보만 있는 경우
        if (string.IsNullOrEmpty(abilityName) && !string.IsNullOrEmpty(randomBlock))
            return randomBlock;

        // 둘 다 있으면: 카드와 비슷하게 "이름\n슬롯 설명"
        return $"{abilityName}\n{randomBlock}";
    }

    //Buffed List
    private void ClearBuffEffectLists()
    {
        if (basicEffectListRoot != null)
        {
            for (int i = basicEffectListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(basicEffectListRoot.GetChild(i).gameObject);
            }
        }

        if (randomEffectListRoot != null)
        {
            for (int i = randomEffectListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(randomEffectListRoot.GetChild(i).gameObject);
            }
        }
    }

    private void AddEffectLine(RectTransform root, string text)
    {
        if (root == null) return;
        if (effectLinePrefab == null) return;
        if (string.IsNullOrEmpty(text)) return;

        var go = Instantiate(effectLinePrefab, root);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;
    }
    private string BuildStatChangeLine(string statName, float delta, string formattedValue)
    {
        if (Mathf.Approximately(delta, 0f)) return null;

        string dir = delta > 0f ? "상승" : "하락";
        return $"{statName} 능력치 {dir} ({formattedValue})";
    }

    private void FillBasicBuffEffects(TowerAmplifier amplifierTower)
    {
        if (basicEffectListRoot == null) return;

        for (int i = basicEffectListRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(basicEffectListRoot.GetChild(i).gameObject);
        }

        if (amplifierTower == null) return;

        var ampData = amplifierTower.AmplifierTowerData;
        if (ampData == null) return;

        if (!amplifierTower.HasAppliedBaseBuffs) return;
       
        // 공격력 (DamageBuff: add, 0.4 -> +40%)
        if (!Mathf.Approximately(ampData.DamageBuff, 0f))
        {
            float delta = ampData.DamageBuff;
            string value = FormatPercentFromAdd(delta); // "+40%"
            string line = BuildStatChangeLine("공격력", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 공속 (FireRateBuff: mul, 1.2 -> +20%)
        if (!Mathf.Approximately(ampData.FireRateBuff, 1f))
        {
            float delta = ampData.FireRateBuff - 1f;
            string value = FormatPercentFromMul(ampData.FireRateBuff);
            string line = BuildStatChangeLine("공격 속도", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 투사체 수 +N
        if (ampData.ProjectileCountBuff != 0)
        {
            float delta = ampData.ProjectileCountBuff;
            string value = delta > 0 ? $"+{delta}" : delta.ToString();
            string line = BuildStatChangeLine("투사체 수", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 타겟 수 +N
        if (ampData.TargetNumberBuff != 0)
        {
            float delta = ampData.TargetNumberBuff;
            string value = delta > 0 ? $"+{delta}" : delta.ToString();
            string line = BuildStatChangeLine("타겟 수", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 충돌 크기 (HitRadiusBuff: add, 0.25 -> +25%)
        if (!Mathf.Approximately(ampData.HitRadiusBuff, 0f))
        {
            float delta = ampData.HitRadiusBuff;
            string value = FormatPercentFromAdd(delta);
            string line = BuildStatChangeLine("충돌 크기", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 비율 관통력 (PercentPenetrationBuff: add, 0.2 -> +20%)
        if (!Mathf.Approximately(ampData.PercentPenetrationBuff, 0f))
        {
            float delta = ampData.PercentPenetrationBuff; // 0.2 → +20%
            float percent = delta * 100f;
            string value = $"{percent:+0.##;-0.##}%";
            string line = BuildStatChangeLine("비율 관통력", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 고정 관통 +N
        if (!Mathf.Approximately(ampData.FixedPenetrationBuff, 0f))
        {
            float delta = ampData.FixedPenetrationBuff;
            string value = delta > 0 ? $"+{delta:0.##}" : $"{delta:0.##}";
            string line = BuildStatChangeLine("고정\n관통력", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }

        // 명중률 (HitRateBuff: mul, 1.2 -> +20%)
        if (!Mathf.Approximately(ampData.HitRateBuff, 1f))
        {
            float delta = ampData.HitRateBuff - 1f;
            string value = FormatPercentFromMul(ampData.HitRateBuff);
            string line = BuildStatChangeLine("명중률", delta, value);
            AddEffectLine(basicEffectListRoot, line);
        }
    }

    private void FillRandomAbilityEffects(TowerAmplifier amplifierTower)
    {
        if (randomEffectListRoot == null) return;

        for (int i = randomEffectListRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(randomEffectListRoot.GetChild(i).gameObject);
        }

        if (amplifierTower == null) return;

        var ampData = amplifierTower.AmplifierTowerData;
        var abilities = amplifierTower.Abilities;
        if (ampData == null || abilities == null || abilities.Count == 0) return;

        if (!amplifierTower.HasAppliedRandomAbilities) return;

        bool hasAnyLine = false;

        foreach (var abilityId in abilities)
        {
            var raRow = DataTableManager.RandomAbilityTable?.Get(abilityId);
            if (raRow == null) continue;

            string abilityName = raRow.RandomAbilityName;
            float effectValue = raRow.SpecialEffectValue;

            var lines = BuildRandomAbilityStatLines(abilityId, abilityName, effectValue);
            if (lines == null || lines.Count == 0)
                continue;

            foreach (var line in lines)
            {
                AddEffectLine(randomEffectListRoot, line);
                hasAnyLine = true;
            }
        }
    }

    private List<string> BuildRandomAbilityStatLines(
    int abilityId,
    string abilityName,
    float effectValue)
    {
        var result = new List<string>();

        switch (abilityId)
        {
            case 200001: // 공격력%
                {
                    string formatted = $"{effectValue:+0;-0}%";
                    string line = BuildStatChangeLine("공격력", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200002: // 공속%
            case 200017: // 공속% (다른 PlaceType 버전)
                {
                    string formatted = $"{effectValue:+0;-0}%";
                    string line = BuildStatChangeLine("공격 속도", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200003: // 비율 관통력
                {
                    string formatted = $"{effectValue:+0;-0}%";
                    string line = BuildStatChangeLine("비율 관통력", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200004: // 고정 관통력
                {
                    string formatted = $"{effectValue:+0.##;-0.##}";
                    string line = BuildStatChangeLine("고정 관통력", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200005: // 둔화 (이동 속도 감소)
                {
                    string formatted = $"{effectValue:+0;-0}%";
                    string line = BuildStatChangeLine("이동 속도 감소", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200006: // 충돌크기
                {
                    string formatted = $"{effectValue:+0;-0}%";
                    string line = BuildStatChangeLine("충돌 크기", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200007: // 연쇄
                {
                    string formatted = $"{effectValue:+0;-0}";
                    string line = BuildStatChangeLine("연쇄 횟수", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200008: // 폭발
                {
                    if (!Mathf.Approximately(effectValue, 0f))
                    {
                        string formatted = $"{effectValue:+0;-0}";
                        string line = BuildStatChangeLine("폭발 효과", effectValue, formatted);
                        if (line != null) result.Add(line);
                    }
                    else
                    {
                        result.Add("폭발 효과 발동");
                    }
                    break;
                }

            case 200009: // 관통
                {
                    string formatted = $"{effectValue:+0;-0}";
                    string line = BuildStatChangeLine("관통 횟수", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200010: // 분열
                {
                    string formatted = $"{effectValue:+0;-0}";
                    string line = BuildStatChangeLine("분열 투사체 수", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200011: // 투사체 수
                {
                    string formatted = $"{effectValue:+0;-0}";
                    string line = BuildStatChangeLine("투사체 수", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200012: // 타겟 수
                {
                    string formatted = $"{effectValue:+0;-0}";
                    string line = BuildStatChangeLine("타겟 수", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200013: // 히트스캔
                {
                    result.Add("히트스캔 공격 활성");
                    break;
                }

            case 200014: // 유도
                {
                    result.Add("유도 탄환 발사");
                    break;
                }

            case 200015: // 유지시간
                {
                    string formatted = $"{effectValue:+0;-0}초";
                    string line = BuildStatChangeLine("투사체 유지 시간", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            case 200016: // 명중률
                {
                    string formatted = $"{effectValue:+0;-0}%";
                    string line = BuildStatChangeLine("명중률", effectValue, formatted);
                    if (line != null) result.Add(line);
                    break;
                }

            default:
                {
                    if (!Mathf.Approximately(effectValue, 0f))
                    {
                        string formatted = $"{effectValue:+0.##;-0.##}";
                        string line = BuildStatChangeLine(abilityName, effectValue, formatted);
                        if (line != null) result.Add(line);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(abilityName))
                            result.Add(abilityName);
                    }
                    break;
                }
        }
        return result;
    }
}