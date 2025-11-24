using System.Collections.Generic;
public enum AmplifierStatKind
{
    None = 0,

    DamagePercent,      // 공격력%
    AttackSpeed,        // 공격 속도%
    ProjectileCount,    // 투사체 수
    TargetCount,        // 타겟 수
    HitRate,            // 명중률
    HitRadius,          // 충돌 크기
    PercentPenetration, // 비율 관통력
    FixedPenetration,   // 고정 관통력
}

public static class SpecialEffectMeta
{
    private static readonly Dictionary<int, AmplifierStatKind> effectToStat
        = new Dictionary<int, AmplifierStatKind>
    {
        // === 능력치 계열 (type=2) ===

        { 1011001, AmplifierStatKind.AttackSpeed },        // 공격 속도%
        { 1011002, AmplifierStatKind.ProjectileCount },    // 투사체 수
        { 1011003, AmplifierStatKind.TargetCount },        // 타겟 수
        { 1011004, AmplifierStatKind.HitRate },            // 명중률

        { 1102001, AmplifierStatKind.DamagePercent },      // 공격력%
        { 1102002, AmplifierStatKind.None },               // 투사체 속도
        { 1102003, AmplifierStatKind.None },               // 가속도 
        { 1102004, AmplifierStatKind.HitRadius },          // 충돌 크기
        { 1102005, AmplifierStatKind.PercentPenetration }, // 비율 관통력
        { 1102006, AmplifierStatKind.FixedPenetration },   // 고정 관통력

        // === 공격 타입/CC 계열 (type=1,3,4,5,6) 
        // { 1101001, AmplifierStatKind.None }, // 관통 횟수
        // { 1101002, AmplifierStatKind.None }, // 연쇄 수
        // { 1101003, AmplifierStatKind.None }, // 폭발 범위
        // { 1101004, AmplifierStatKind.None }, // 유도 확인
        // { 1101005, AmplifierStatKind.None }, // 분열
        // { 1101006, AmplifierStatKind.None }, // 장판
        // { 1104001, AmplifierStatKind.None }, // 둔화
        // { 1105001, AmplifierStatKind.None }, // 집탄률
        // { 1106001, AmplifierStatKind.None }, // 유지시간
    };
    public static AmplifierStatKind GetStatKind(int specialEffectId)
    {
        if (effectToStat.TryGetValue(specialEffectId, out var kind))
        {
            return kind;
        }

        return AmplifierStatKind.None;
    }
}