public enum FxId
{
    // Combat (World)
    BasicHit,                 // Basic Hit
    Death_RareOrMidBoss,       // Smoke/Fire/Artillery 조합(시퀀스 추천)
    Death_FinalBoss,           // Smoke/Fire/Artillery 조합(시퀀스 추천)

    // UI (Particles / simple)
    UI_Tap,                    // Ef_Tap
    UI_ButtonPress,            // (확정되면)
    UI_WaveChange,             // (없음 -> 제작 필요)
    UI_NewTowerBadge,          // New! 이펙트
    UI_ReinforceStarsTwinkle,  // 별 반짝

    // UI (Panel animations / sequences)
    UI_StageEndOpen,           // Prefab_StageEnd (Show/Anim)
    UI_AugmentOpen,            // 결과창 연출 재사용
    UI_QuasarOpen,
    UI_DeployOpen,
    UI_BossRaidWarning,        // Title_LineTopBottom_03_Red 등

    // Deploy (World/UI 혼합)
    Tower_Install,             // Shine_Green
    Tower_Move,                // Shine_Green
    Tower_Select,              // 점점 커지는 애니(보통 UI/월드 선택 강조)
    Tower_SwapSlotHighlight,   // 슬롯 커짐 강조(이건 UI 애니메이션 성격)

    // Quasar (World/UI)
    Quasar_AddMaxTower,        // FX_GlowSpot
    Quasar_RandomAbilityGet,   // FX_GlowSpot

    // Augment
    Augment_NewTowerPick,      // 확보(프리팹명 나중에)
    Augment_TowerUpgrade,      // Aura_acceleration

    // Other
    Deploy_Remove,             // Leaf_Red
    Stage_Clear_Fireworks,     // 폭죽
    Planet_CardOutline,        // 카드 테두리 이펙트
    Planet_LevelUp,            // 흰 빛 퍼짐
    Planet_StarUp,             // 흰 빛 퍼짐
    Planet_CardUnlock,         // 0->1성 해금 연출
}
