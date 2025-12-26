using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class TowerAttackDebugInfoExtensions
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    public static string GetDebugInfo(this TowerAttack t)
    {
        if (t == null) return "<TowerAttack is null>";

        StringBuilder sb = new StringBuilder();

        // public
        var towerData = t.AttackTowerData;
        int reinforceLevel = t.ReinforceLevel;

        // private (reflection)
        var currentProjectileData = GetField<ProjectileData>(t, "currentProjectileData");
        var originalProjectileData = GetField<ProjectileData>(t, "originalProjectileData");

        float damageBuffMul = GetField<float>(t, "damageBuffMul", 1f);
        float damageAbilityMul = GetField<float>(t, "damageAbilityMul", 1f);
        float damageBuffFromUpgrade = GetField<float>(t, "damageBuffFromUpgrade", 0f);
        var damageAbilitySources = GetField<List<float>>(t, "damageAbilitySources");

        float fireRateBuffMul = t.fireRateBuffMul;
        float fireRateAbilityMul = t.fireRateAbilityMul;
        float towerUpgradeFireRateMul = GetField<float>(t, "towerUpgradeFireRateMul", 0f);
        var fireRateAbilitySources = GetField<List<float>>(t, "fireRateAbilitySources");

        int baseProjectileCount = t.BaseProjectileCount;
        int projectileCountFromAmplifier = t.ProjectileCountFromAmplifier;
        int projectileCountFromAbility = t.ProjectileCountFromAbility;
        int projectileCountFromUpgrade = GetField<int>(t, "projectileCountFromUpgrade", 0);

        float accuracyFromAmplifier = GetField<float>(t, "accuracyFromAmplifier", 0f);
        float accuracyBuffAdd = t.AccuracyBuffAdd;

        float percentPenetrationFromAmplifier = GetField<float>(t, "percentPenetrationFromAmplifier", 0f);
        float percentPenetrationFromAbility = GetField<float>(t, "percentPenetrationFromAbility", 0f);
        var percentPenAbilitySources = GetField<List<float>>(t, "percentPenAbilitySources");

        float fixedPenetrationFromAmplifier = GetField<float>(t, "fixedPenetrationFromAmplifier", 0f);
        float fixedPenetrationBuffAdd = t.FixedPenetrationBuffAdd;

        int targetNumberFromAmplifier = t.TargetNumberFromAmplifier;
        int targetNumberFromAbility = t.TargetNumberBuffAdd;
        int totalExtraTargets = t.TotalTargetNumberExtra;

        float ampHitRadiusMul = GetField<float>(t, "ampHitRadiusMul", 1f);
        float hitRadiusBuffMul = t.HitRadiusBuffMul;
        var hitRadiusAbilitySources = GetField<List<float>>(t, "hitRadiusAbilitySources");

        var activeAmplifierBuffs = GetField<List<AmplifierTowerDataSO>>(t, "activeAmplifierBuffs");

        var baseAbilityIds = GetField<List<int>>(t, "baseAbilityIds");
        var amplifierAbilityIds = GetField<Dictionary<TowerAmplifier, List<int>>>(t, "amplifierAbilityIds");
        var ownedAbilityIds = GetField<List<int>>(t, "ownedAbilityIds");

        var abilities = t.Abilities;

        // 타워 기본 정보
        sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        if (towerData != null)
        {
            sb.AppendLine($"타워 타입: {towerData.towerId.Replace("\\n", " ")} (ID: {towerData.towerIdInt})");
        }
        sb.AppendLine($"강화 레벨: {reinforceLevel}");
        sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        // 공격력
        sb.AppendLine();
        sb.AppendLine("🔥 공격력 (DAMAGE)");

        float baseAtk = originalProjectileData != null ? originalProjectileData.Attack : 0f;
        float reinforcedAtk = currentProjectileData != null ? currentProjectileData.Attack : 0f;
        sb.AppendLine($"  기본 (Base):                  {baseAtk:F2}");
        if (!Mathf.Approximately(reinforcedAtk, baseAtk))
        {
            float bonus = reinforcedAtk - baseAtk;
            sb.AppendLine($"  강화 보너스 (Reinforce):       +{bonus:F2}  (Lv.{reinforceLevel})");
        }
        sb.AppendLine($"  최종 공격력 (FINAL):           {reinforcedAtk:F2}");

        if (!Mathf.Approximately(damageBuffMul, 1f))
        {
            float percent = (damageBuffMul - 1f) * 100f;
            sb.AppendLine($"  증폭타워 배율 (Amplifier):     ×{damageBuffMul:F3}  ({FormatSigned(percent, 1)}%)");
            if (activeAmplifierBuffs != null && activeAmplifierBuffs.Count > 0)
            {
                foreach (var amp in activeAmplifierBuffs)
                {
                    if (amp == null) continue;
                    float buffPercent = amp.DamageBuff * 100f;
                    sb.AppendLine($"    • {amp.name}: {FormatSigned(buffPercent, 1)}%");
                }
            }
        }

        if (!Mathf.Approximately(damageAbilityMul, 1f))
        {
            float percent = (damageAbilityMul - 1f) * 100f;
            sb.AppendLine($"  자체 능력 배율 (Self Ability): ×{damageAbilityMul:F3}  ({FormatSigned(percent, 1)}%)");
            if (damageAbilitySources != null && damageAbilitySources.Count > 0)
            {
                sb.AppendLine($"    • {damageAbilitySources.Count}개 소스:");
                for (int i = 0; i < damageAbilitySources.Count; i++)
                {
                    float srcPercent = damageAbilitySources[i] * 100f;
                    sb.AppendLine($"      [{i + 1}] {FormatSigned(srcPercent, 1)}%");
                }
            }
        }

        if (!Mathf.Approximately(damageBuffFromUpgrade, 0f))
        {
            float percent = damageBuffFromUpgrade * 100f;
            sb.AppendLine($"  외부 업그레이드 (Upgrade):     +{percent:F1}%");
        }

        float finalDamage = reinforcedAtk * damageBuffMul * damageAbilityMul * (1f + damageBuffFromUpgrade);
        sb.AppendLine($"  ─────────────────────────────────");
        sb.AppendLine($"  최종 공격력 (FINAL):           {finalDamage:F2}");

        // 공격 속도
        sb.AppendLine();
        sb.AppendLine("⚡ 공격 속도 (FIRE RATE)");

        float baseFR = towerData != null ? towerData.fireRate : 0f;
        sb.AppendLine($"  기본 (Base):                  {baseFR:F2} /s");

        if (!Mathf.Approximately(fireRateBuffMul, 0f))
        {
            float percent = fireRateBuffMul * 100f;
            sb.AppendLine($"  증폭타워 보너스 (Amplifier):   {FormatSigned(percent, 1)}%");
        }

        if (!Mathf.Approximately(fireRateAbilityMul, 0f))
        {
            float percent = fireRateAbilityMul * 100f;
            sb.AppendLine($"  자체 능력 보너스 (Ability):    {FormatSigned(percent, 1)}%");
            if (fireRateAbilitySources != null && fireRateAbilitySources.Count > 0)
            {
                sb.AppendLine($"    • {fireRateAbilitySources.Count}개 소스:");
                for (int i = 0; i < fireRateAbilitySources.Count; i++)
                {
                    float srcPercent = fireRateAbilitySources[i] * 100f;
                    sb.AppendLine($"      [{i + 1}] {FormatSigned(srcPercent, 1)}%");
                }
            }
        }

        if (!Mathf.Approximately(towerUpgradeFireRateMul, 0f))
        {
            float percent = towerUpgradeFireRateMul * 100f;
            sb.AppendLine($"  외부 업그레이드 (Upgrade):     {FormatSigned(percent, 1)}%");
        }

        sb.AppendLine($"  ─────────────────────────────────");
        sb.AppendLine($"  최종 공격속도 (FINAL):         {t.CurrentFireRate:F2} /s");

        // 투사체 개수
        sb.AppendLine();
        sb.AppendLine("🎯 투사체 개수 (PROJECTILE COUNT)");
        sb.AppendLine($"  기본 (Base):                  {baseProjectileCount}");

        if (projectileCountFromAmplifier > 0)
            sb.AppendLine($"  증폭타워 (Amplifier):          +{projectileCountFromAmplifier}");

        if (projectileCountFromAbility > 0)
            sb.AppendLine($"  자체 능력 (Ability):           +{projectileCountFromAbility}");

        if (projectileCountFromUpgrade > 0)
            sb.AppendLine($"  외부 업그레이드 (Upgrade):     +{projectileCountFromUpgrade}");

        sb.AppendLine($"  ─────────────────────────────────");
        sb.AppendLine($"  최종 투사체 개수 (FINAL):      {t.CurrentProjectileCount}");

        // 명중률
        sb.AppendLine();
        sb.AppendLine("🎲 명중률 (ACCURACY)");

        float baseAcc = towerData != null ? towerData.Accuracy : 0f;
        sb.AppendLine($"  기본 (Base):                  {baseAcc:F1}%");

        if (!Mathf.Approximately(accuracyFromAmplifier, 0f))
            sb.AppendLine($"  증폭타워 (Amplifier):          {FormatSigned(accuracyFromAmplifier, 1)}%");

        if (!Mathf.Approximately(accuracyBuffAdd, 0f))
            sb.AppendLine($"  자체 능력 (Ability):           {FormatSigned(accuracyBuffAdd, 1)}%");

        sb.AppendLine($"  ─────────────────────────────────");
        sb.AppendLine($"  최종 명중률 (FINAL):           {t.FinalHitRate:F1}%");

        // 관통
        sb.AppendLine();
        sb.AppendLine("🔓 관통 (PENETRATION)");

        sb.AppendLine("  [퍼센트 관통 (Percent)]");
        float basePercent = currentProjectileData != null ? currentProjectileData.RatePenetration : 0f;
        sb.AppendLine($"    기본 (Base):                {basePercent:F1}%");

        if (!Mathf.Approximately(percentPenetrationFromAmplifier, 0f))
            sb.AppendLine($"    증폭타워 (Amplifier):        {percentPenetrationFromAmplifier * 100f:F1}%");

        if (!Mathf.Approximately(percentPenetrationFromAbility, 0f))
        {
            sb.AppendLine($"    자체 능력 (Ability):         {percentPenetrationFromAbility * 100f:F1}%");
            if (percentPenAbilitySources != null)
            {
                sb.AppendLine($"      • {percentPenAbilitySources.Count}개 소스:");
                for (int i = 0; i < percentPenAbilitySources.Count; i++)
                {
                    float srcPercent = percentPenAbilitySources[i] * 100f;
                    sb.AppendLine($"        [{i + 1}] {srcPercent:F1}%");
                }
            }
        }

        sb.AppendLine("  [고정 관통 (Fixed)]");
        float baseFixed = currentProjectileData != null ? currentProjectileData.FixedPenetration : 0f;
        sb.AppendLine($"    기본 (Base):                {baseFixed:F1}");

        if (!Mathf.Approximately(fixedPenetrationFromAmplifier, 0f))
            sb.AppendLine($"    증폭타워 (Amplifier):        +{fixedPenetrationFromAmplifier:F1}");

        if (!Mathf.Approximately(fixedPenetrationBuffAdd, 0f))
            sb.AppendLine($"    자체 능력 (Ability):         +{fixedPenetrationBuffAdd:F1}");

        // 타겟 개수
        sb.AppendLine();
        sb.AppendLine("👥 타겟 개수 (TARGET COUNT)");

        float baseTarget = currentProjectileData != null ? currentProjectileData.TargetNum : 1f;
        sb.AppendLine($"  기본 (Base):                  {baseTarget:F0}");

        if (targetNumberFromAmplifier > 0)
            sb.AppendLine($"  증폭타워 (Amplifier):          +{targetNumberFromAmplifier}");

        if (targetNumberFromAbility > 0)
            sb.AppendLine($"  자체 능력 (Ability):           +{targetNumberFromAbility}");

        if (totalExtraTargets > 0)
        {
            sb.AppendLine($"  ─────────────────────────────────");
            sb.AppendLine($"  추가 타겟 (Extra):             +{totalExtraTargets}");
        }

        // 히트 반경
        sb.AppendLine();
        sb.AppendLine("📏 히트 반경 (HIT RADIUS)");

        float baseRadius = currentProjectileData != null ? currentProjectileData.CollisionSize : 0f;
        sb.AppendLine($"  기본 (Base):                  {baseRadius:F2}");

        if (!Mathf.Approximately(ampHitRadiusMul, 1f))
        {
            float percent = (ampHitRadiusMul - 1f) * 100f;
            sb.AppendLine($"  증폭타워 배율 (Amplifier):     ×{ampHitRadiusMul:F3}  ({FormatSigned(percent, 1)}%)");
        }

        if (hitRadiusAbilitySources != null && hitRadiusAbilitySources.Count > 0)
        {
            sb.AppendLine($"  자체 능력 소스 (Ability):      {hitRadiusAbilitySources.Count}개");
            for (int i = 0; i < hitRadiusAbilitySources.Count; i++)
            {
                float srcPercent = hitRadiusAbilitySources[i] * 100f;
                sb.AppendLine($"    [{i + 1}] {FormatSigned(srcPercent, 1)}%");
            }
        }

        sb.AppendLine($"  ─────────────────────────────────");
        sb.AppendLine($"  최종 배율 (FINAL Mul):         ×{hitRadiusBuffMul:F3}");

        // 투사체 속도
        sb.AppendLine();
        sb.AppendLine("🚀 투사체 속도 (PROJECTILE SPEED)");

        float baseSpeed = currentProjectileData != null ? currentProjectileData.ProjectileSpeed : 0f;
        sb.AppendLine($"  기본 (Base):                  {baseSpeed:F2}");

        var projectileSpeedAbilityMul = GetField<float>(t, "projectileSpeedAbilityMul", 1f);
        var projectileSpeedAbilitySources = GetField<List<float>>(t, "projectileSpeedAbilitySources");

        if (!Mathf.Approximately(projectileSpeedAbilityMul, 1f))
        {
            float percent = (projectileSpeedAbilityMul - 1f) * 100f;
            sb.AppendLine($"  자체 능력 배율 (Ability):      ×{projectileSpeedAbilityMul:F3}  ({FormatSigned(percent, 1)}%)");

            if (projectileSpeedAbilitySources != null && projectileSpeedAbilitySources.Count > 0)
            {
                sb.AppendLine($"    • {projectileSpeedAbilitySources.Count}개 소스:");
                for (int i = 0; i < projectileSpeedAbilitySources.Count; i++)
                {
                    float srcPercent = projectileSpeedAbilitySources[i] * 100f;
                    sb.AppendLine($"      [{i + 1}] {FormatSigned(srcPercent, 1)}%");
                }
            }
        }

        float finalSpeed = baseSpeed * projectileSpeedAbilityMul;
        sb.AppendLine($"  ─────────────────────────────────");
        sb.AppendLine($"  최종 투사체 속도 (FINAL):      {finalSpeed:F2}");

        // ✅ 어빌리티 상세 정보
        sb.AppendLine();
        sb.AppendLine("✨ 어빌리티 (ABILITIES)");

        if (baseAbilityIds != null && baseAbilityIds.Count > 0)
        {
            sb.AppendLine($"  [자체 어빌리티 ({baseAbilityIds.Count}개)]");
            foreach (var abilityId in baseAbilityIds)
            {
                var abilityData = DataTableManager.RandomAbilityTable?.Get(abilityId);
                string abilityName = abilityData != null ? abilityData.RandomAbilityName : $"ID:{abilityId}";

                // ✅ Unlock Ability 상세 정보 추가
                if (IsUnlockAbility(abilityId) && abilityData != null)
                {
                    sb.AppendLine($"    • {abilityName} (ID: {abilityId}) [Unlock Ability]");
                    AppendUnlockAbilityDetails(sb, abilityData, reinforceLevel);
                }
                else
                {
                    sb.AppendLine($"    • {abilityName} (ID: {abilityId})");
                }
            }
        }

        if (amplifierAbilityIds != null && amplifierAbilityIds.Count > 0)
        {
            sb.AppendLine($"  [증폭타워로부터 받은 어빌리티 ({amplifierAbilityIds.Count}개 소스)]");
            foreach (var kv in amplifierAbilityIds)
            {
                var source = kv.Key;
                var list = kv.Value;
                if (source == null || list == null) continue;

                sb.AppendLine($"    • 소스: 슬롯 {source.SelfIndex} (강화 Lv.{source.ReinforceLevel})");
                foreach (var abilityId in list)
                {
                    var abilityData = DataTableManager.RandomAbilityTable?.Get(abilityId);
                    string abilityName = abilityData != null ? abilityData.RandomAbilityName : $"ID:{abilityId}";
                    sb.AppendLine($"      - {abilityName} (ID: {abilityId})");
                }
            }
        }

        if (ownedAbilityIds != null && ownedAbilityIds.Count > 0)
        {
            sb.AppendLine($"  [카드로 획득한 어빌리티 ({ownedAbilityIds.Count}개)]");
            var uniqueOwned = new HashSet<int>(ownedAbilityIds);
            foreach (var abilityId in uniqueOwned)
            {
                var abilityData = DataTableManager.RandomAbilityTable?.Get(abilityId);
                string abilityName = abilityData != null ? abilityData.RandomAbilityName : $"ID:{abilityId}";

                int count = 0;
                foreach (var id in ownedAbilityIds)
                    if (id == abilityId) count++;

                sb.AppendLine($"    • {abilityName} (ID: {abilityId}) ×{count}");
            }
        }

        // 타입 1 능력 배율
        sb.AppendLine();
        sb.AppendLine("타입 1 능력 배율 (TYPE 1 ABILITY MULTIPLIERS)");

        if (abilities != null && abilities.Contains(200011))
        {
            float mul = GetAbilityDamageMultiplierLike(t, 200011);
            sb.AppendLine($"  투사체 수 (200011):            {mul:F2}");
            sb.AppendLine($"    각 투사체 데미지 = 기본 × {mul:F2}");
        }

        if (abilities != null && abilities.Contains(200012))
        {
            float mul = GetAbilityDamageMultiplierLike(t, 200012);
            sb.AppendLine($"  타겟 수 (200012):              {mul:F2}");
            sb.AppendLine($"    각 타겟 데미지 = 기본 × {mul:F2}");
        }

        if (abilities != null && abilities.Contains(200009))
        {
            float mul = GetAbilityDamageMultiplierLike(t, 200009);
            sb.AppendLine($"  관통 (200009):                 {mul:F2}");
            sb.AppendLine($"    2번째 타겟부터 데미지 = 기본 × {mul:F2}");
        }

        if (abilities != null && abilities.Contains(200008))
        {
            float mul = GetAbilityDamageMultiplierLike(t, 200008);
            sb.AppendLine($"  폭발 (200008):                 {mul:F2}");
            sb.AppendLine($"    폭발 데미지 = 기본 × {mul:F2}");
        }

        if (abilities != null && abilities.Contains(200007))
        {
            float mul = GetAbilityDamageMultiplierLike(t, 200007);
            sb.AppendLine($"  연쇄 (200007):                 {mul:F2}");
            sb.AppendLine($"    1번째 연쇄 = 기본 × {mul:F2}");
            sb.AppendLine($"    2번째 연쇄 = 기본 × {(mul * mul):F2}");
            sb.AppendLine($"    3번째 연쇄 = 기본 × {(mul * mul * mul):F2}");
        }

        if (abilities != null && abilities.Contains(200010))
        {
            float mul = GetAbilityDamageMultiplierLike(t, 200010);
            sb.AppendLine($"  분열 (200010):                 {mul:F2}");
            sb.AppendLine($"    각 분열 투사체 데미지 = 기본 × {mul:F2}");
        }

        sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        return sb.ToString();
    }

    // ✅ Unlock Ability 판별
    private static bool IsUnlockAbility(int abilityId)
    {
        return abilityId >= 200018 && abilityId <= 200023;
    }

    // ✅ Unlock Ability 상세 정보 출력
    private static void AppendUnlockAbilityDetails(StringBuilder sb, RandomAbilityData data, int reinforceLevel)
    {
        sb.AppendLine($"      → 복합 효과 (강화 Lv.{reinforceLevel}):");

        if (TowerReinforceManager.Instance == null)
        {
            sb.AppendLine($"        [TowerReinforceManager not available]");
            return;
        }

        // ✅ TowerReinforceManager를 사용하여 강화합 계산
        var sum = TowerReinforceManager.Instance.GetRandomAbilityReinforceSumForAbility(data.RandomAbility_ID, reinforceLevel);

        // Effect 1
        if (data.SpecialEffect_ID != 0)
        {
            var effectName = data.RandomAbilityName;
            var baseValue = data.SpecialEffectValue;
            var addValue = sum.GetAdd(data.SpecialEffect_ID);
            var finalValue = baseValue + addValue;

            if (reinforceLevel > 0)
            {
                sb.AppendLine($"        [1] {effectName}: {baseValue:F1} → {finalValue:F1} (+{addValue:F1})");
            }
            else
            {
                sb.AppendLine($"        [1] {effectName}: {finalValue:F1}");
            }
        }

        // Effect 2
        if (data.SpecialEffect2_ID.HasValue && data.SpecialEffect2_ID.Value != 0)
        {
            var effectName = data.RandomAbility2Name;
            var baseValue = data.SpecialEffect2Value ?? 0f;
            var addValue = sum.GetAdd(data.SpecialEffect2_ID.Value);
            var finalValue = baseValue + addValue;

            if (reinforceLevel > 0)
            {
                sb.AppendLine($"        [2] {effectName}: {baseValue:F1} → {finalValue:F1} (+{addValue:F1})");
            }
            else
            {
                sb.AppendLine($"        [2] {effectName}: {finalValue:F1}");
            }
        }

        // Effect 3
        if (data.SpecialEffect3_ID.HasValue && data.SpecialEffect3_ID.Value != 0)
        {
            var effectName = data.RandomAbility3Name;
            var baseValue = data.SpecialEffect3Value ?? 0f;
            var addValue = sum.GetAdd(data.SpecialEffect3_ID.Value);
            var finalValue = baseValue + addValue;

            if (reinforceLevel > 0)
            {
                sb.AppendLine($"        [3] {effectName}: {baseValue:F1} → {finalValue:F1} (+{addValue:F1})");
            }
            else
            {
                sb.AppendLine($"        [3] {effectName}: {finalValue:F1}");
            }
        }
    }

    private static float GetAbilityDamageMultiplierLike(TowerAttack t, int abilityId)
    {
        if (t == null) return 1f;

        var abilities = t.Abilities;
        if (abilities == null || !abilities.Contains(abilityId))
            return 1f;

        if (!DataTableManager.IsInitialized)
            return 1f;

        var ra = DataTableManager.RandomAbilityTable?.Get(abilityId);
        if (ra == null)
            return 1f;

        if (ra.RandomAbilityType == 1)
        {
            if (TowerReinforceManager.Instance == null)
                return 1f;

            return TowerReinforceManager.Instance
                .GetFinalSuperValueForAbility(abilityId, t.ReinforceLevel);
        }

        return 1f;
    }

    private static string FormatSigned(float value, int decimals)
    {
        string fmt = decimals <= 0 ? "+0;-0;0" : "+0." + new string('0', decimals) + ";-0." + new string('0', decimals) + ";0." + new string('0', decimals);
        return value.ToString(fmt);
    }

    private static T GetField<T>(object obj, string fieldName, T fallback = default)
    {
        try
        {
            if (obj == null) return fallback;

            Type type = obj.GetType();
            FieldInfo fi = null;

            while (type != null)
            {
                fi = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fi != null) break;
                type = type.BaseType;
            }

            if (fi == null) return fallback;

            object val = fi.GetValue(obj);
            if (val == null) return fallback;

            if (val is T typed) return typed;
            return fallback;
        }
        catch
        {
            return fallback;
        }
    }

#else
    public static string GetDebugInfo(this TowerAttack t) => string.Empty;
#endif
}