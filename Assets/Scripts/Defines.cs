using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class TagName
{
    public static readonly string Planet = "Planet";
    public static readonly string Projectile = "Projectile";
    public static readonly string DropItem = "DropItem";
    public static readonly string Enemy = "Enemy";
    public static readonly string CenterStone = "CenterStone";
    public static readonly string PatternLine = "PatternLine";
    public static readonly string ProjectilePoolManager = "ProjectilePoolManager";
    public static readonly string MainCanvas = "MainCanvas";
    public static readonly string Boss = "Boss";
    public static readonly string BattleUI = "BattleUI";
    public static readonly string ChangedVisual = "ChangedVisual";
    public static readonly string ChangeVisual = "ChangeVisual";
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string PlanetPanelUI = "PlanetPanelUI";
}

public static class ObjectName
{
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string Enemy = "EnemySample";
    public static readonly string BossEnemy = "EnemyBossSample";

    public static readonly string ProjectilePrefab = "Projectile_Sample";
    public static readonly string ProjectileGun = "Projectile_GunTower";
    public static readonly string ProjectileMissile = "Projectile_MissileTower";
    public static readonly string ProjectileGatling = "Projectile_GatlingTower";
    public static readonly string ProjectileShoot = "Projectile_ShootTower";
    public static readonly string ProjectileSniper = "Projectile_SniperTower";
    
    public static readonly string Projectile = "Projectile";
    public static readonly string ChainEffect = "ChainEffect";
    public static readonly string MeteorChild = "MeteorChild";
    public static readonly string Explosion = "Explosion";
    public static readonly string HitScan = "HitScan";
    public static readonly string Lazer = "Lazer";
}

public static class SceneName
{
    public static readonly string LoadingScene = "LoadingScene";
    public static readonly string LoginScene = "LoginScene";
    public static readonly string BattleScene = "BattleScene";
    public static readonly string EnemyTestScene = "EnemyTestScene";
    public static readonly string CameraTestScene = "CameraTestScene";
    public static readonly string UiTestScene = "UiTestScene";
    public static readonly string StageSelectScene = "StageSelectScene";
    public static readonly string AsyncRaidTestScene = "AsyncRaidTestScene";
    public static readonly string LobbyScene = "LobbyScene";
    public static readonly string BalanceTestScene = "BalanceTestScene";
}

public static class CategoryName
{
    public static readonly string Gacha = "가챠";
    public static readonly string Others = "다른 아이템 분류";
}

public enum AbilityId
{
    AttackDamage = 200001,
    AttackSpeed,
    PercentPenetration,
    FixedPanetration,
    Slow,
    CollisionSize,
    Chain,
    Explosion,
    Pierce,
    Split,
    ProjectileCount,
    TargetCount,
    Hitscan,
    Homing,
    Duration,
    Accuracy,
    AttackSpeedOneTarget,
    AtkSpeedAtkHitSizeUnlock = 200018,
    AtkSpeedHighUnlock = 200019,
    AtkProjSpeedUnlock = 200020,
    AccuracyHomingUnlock = 200021,
    ExplosionRangePierceUnlock = 200022,
    HitSizeChainUnlock = 200023,
}

public enum AttackTowerId
{
    basicGun = 1000001,
    ShootGun = 1000002,
    Gattling = 1001001,
    Lazer = 1001002,
    Sniper = 1002001,        
    Missile = 1002002,
    
}

public enum PrefabType
{
    Enemy,
    PatternProjectile
}

public enum ProjectileType
{
    Normal,
    Homing,
}

public enum ExecutionTrigger
{
    OnPatternLine,
    OnInterval,
    Immediate,
    OnHealthPercentage,
    ChildrenAlive,
    OnHit,
    OnOrbitReached,
}

public enum MoveType
{
    StraightDown,
    Homing,
    Chase,
    TwoPhaseHomingMovement = 4,
    TwoPhaseDownMovement = 9,
    DescendAndStopMovement,
    Revolution = 11,
    Side,
    FollowParent,
}

public enum PatternIds
{
    None = 0,
    SimpleShot = 2001,
    DaphnisMeteorClusterSummon = 4100001,
    DaphnisEleteMeteorClusterSummon,
    TitanMeteorClusterSummon,
    TitanEleteMeteorClusterSummon,
    TitanLazer,
    SaturnLazer,
    SaturnMeteorClusterSummon,
    SaturnEleteMeteorClusterSummon,
    SaturnMeteorRingSummon,
    TitanRevolution,
    HomingMeteorCluster,
    ChaseMeteorCluster,
    NereidDiaSummon,
    NereidReflectShield,
    NeptuneChaseDiaSummon,
    NeptuneBigDiaSummon,
    NeptuneFrontDiaSummon,
    EliteDiaReflectShield,
    EliteBigDiaReflectShield,
    UFOLazer,
    GalaxyStarShot,
    SpaceWarmLazer,
    SpaceWarmGravityShot,
    FireChildHitChangeSpeedChase,
    FireEyeShootFire,
    BigFireEyeSummonFireChild,
    BigFireEyeSummonFireEye,
    BigFireEyeShootBigFire,
    BigFireEyeFirePillar,
    SunSummonFireChild,
    SunShootFire,
    empty,
    SunParabolicShot,
    BlackHolePhotonEnergy,
    BlackHoleMiniBlackHoleSummon,
    BlackHoleLazer,
    BlackHoleExplosionSummon,
    WhiteHoleSpaceWarmSummon,
    WhiteHoleShoot,
    ConstellationShoot,
    ExplosionEnemy,
}

public enum TutorialPoint
{
    TopBig,
    TopMidium,
    TopRight,
    TopRightTwo,
    TopLeftSmall,
    CenterMidium,
    CenterMidiumTwo,
    BottomBig,
}

public enum ShopCategory
{
    Gacha,
    Others,
}

public enum RewardType
{
    Currency = 1,
    EnhanceItem,
    PlanetPiece,
    Planet = 10,
}

public enum CurrencyType
{
    Gold = 701,
    FreeDia,
    FreePlusChargedDia,
    ChargedDia,
}

public enum PlanetType
{
    BasePlanet = 300001,
    HealthPlanet,
    DefensePlanet,
    ShieldPlanet,
    BloodAbsorbPlanet,
    ExpPlanet,
    HealthRegenerationPlanet,
}

public enum PlanetPieceType
{
    HealthPlanetPiece = 0,
    DefensePlanetPiece,
    ShieldPlanetPiece,
    BloodAbsorbPlanetPiece,
    ExpPlanetPiece,
    HealthRegenerationPlanetPiece,
    CommonPlanetPiece,
}

public enum Currency
{
    Gold = 711101,
    FreeDia = 711201,
    ChargedDia = 711202,
}

public enum PlanetAbilityType
{
    None = 0,
    Health,
    Defense,
    HealthPercentage,
    DefensePercentage,
    Shield,
    Drain,
    RegenerationHP,
    ExperienceRate,

}

public static class DataTableIds
{
    public static readonly string Item = "ItemTable";
    public static readonly string Enemy = "EnemyTable";
    public static readonly string Combine = "CombineTable";
    public static readonly string Wave = "WaveTable";
    public static readonly string Projectile = "ProjectileTable";
    public static readonly string Pattern = "PatternTable";
    public static readonly string MinionSpawn = "MinionSpawnTable";
    public static readonly string RandomAbility = "RandomAbilityTable";
    public static readonly string RandomAbilityGroup = "RandomAbilityGroupTable";
    public const string AttackTower = "AttackTower";
    public const string BuffTower = "BuffTowerTable";
    public const string SpecialEffectCombination = "SpecialEffectCombinationTable";
    public const string SpecialEffect = "SpecialEffectTable";
    public static readonly string PlanetLevelUp = "PlanetLevelUpTable";
    public const string TowerReinforceUpgrade = "TowerReinforceUpgradeTable";
    public static readonly string BuffTowerReinforceUpgrade = "BuffTowerReinforceUpgradeTable";
    public static readonly string AsyncPlanet = "AsyncPlanetTable";
    public static readonly string TowerExplain = "TowerExplainTable";
    public static readonly string Currency = "CurrencyTable";
    public static readonly string Draw = "DrawTable";
    public static readonly string Reward = "RewardTable";
    public static readonly string planet = "PlanetTable";
    public static readonly string RandomAbilityText = "RandomAbilityTextTable";
    public static readonly string RandomAbilityReinforceUpgrade = "RandomAbilityReinforceUpgradeTable";
    public static readonly string TowerUpgrade = "TowerUpgradeTable";
    public static readonly string TowerUpgradeAbilityUnlock = "TowerUpgradeAbilityUnlockTable";
    public static readonly string Skill = "SkillTable";
    public static readonly string Stage = "StageTable";
    public static readonly string PlanetLvUpgrade = "PlanetLvUpgradeTable";
    public static readonly string PlanetStarUpgrade = "PlanetStarUpgradeTable";
    public static readonly string LobbyString = "LobbyString";
    public static readonly string SpecialEffectText = "SpecialEffectTextTable";
    public static readonly string PlanetText = "PlanetTextTable";
    public static readonly string StageReward = "StageRewardTable";
}

public static class Variables
{
    public static int Stage {get; set;} = 3;
    public static int planetId {get; set;} = 300001;

    private static int quasar = 1;
    public static int Quasar
    {
        get => quasar;
        set
        {
            quasar = value;
            if(quasar < 0)
            {
                quasar = 0;
            }
            
            OnQuasarChanged?.Invoke();
        }
    }
    public static event Action OnQuasarChanged;
    public static Enemy LastBossEnemy {get; set;} = null;
    public static Enemy MiddleBossEnemy {get; set;} = null;
    public static GameObject TestBossEnemyObject {get; set;} = null;

    public static void Reset()
    {
        Quasar = 1;
        LastBossEnemy = null;
        MiddleBossEnemy = null;
    }

    public static bool IsTestMode {get; set;} = false;
}

public static class AddressLabel
{
    public static readonly string Prefab = "Prefab";
    public static readonly string PoolObject = "PoolObject";
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string Lazer = "Lazer";
    public static readonly string EnemyLazer = "EnemyLazer";
    public static readonly string Texture = "Texture";
    public static readonly string Mesh = "Mesh";
}

public static class DatabaseRef
{
    public static readonly string UserProfiles = "users";
    public static readonly string UserPlanets = "userplanets";
    public static readonly string UserTowers = "usertowers";
    public static readonly string UserAttackPowers = "userattackpowers";
    public static readonly string UserTowerUpgrades = "usertowerupgrades";
    public static readonly string UserStageData = "userstageclear";
}

public static class PrintedAbility
{
    public static readonly HashSet<int> SpecialRandomAbilityIds = new HashSet<int>
    {
        200007, // 연쇄
        200008, // 폭발
        200009, // 관통
        200010, // 분열
        200013, // 히트스캔
        200014  // 유도
    };
}

public enum SpecialEffectId
{
    AttackSpeed = 1011001,
    ProjectileCount,
    TargetCount,
    Accuracy,
    Pierce = 1101001,
    Chain,
    Explosion,
    Homing,
    Split,
    FloorAttack,
    Attack = 1102001,
    ProjectileSpeed,
    Acceleration,
    HitSize,
    RatePenetration,
    FixedPenetration,
    Slow = 1104001,
    Grouping = 1105001,
    Duration = 1106001,

}

public class GameStrings
{
    public static readonly string TowerSetting = "배치";
    public static readonly string TowerUpgrade = "증강";
    public static readonly string TowerDelete = "제거";
    public static readonly string QuasarItemUsed = "퀘이사";
    public static readonly string QuasarItemSkipTitle = "퀘이사 미선택";
    public static readonly string QuasarItemSkipped = "퀘이사 아이템을 사용하지 않고\n스킵하시겠습니까?";
    public static readonly string QuasarItemDeleted = "(스킵하면 퀘이사 아이템이 사라집니다.)";
    public static readonly string TowerUpgradeSkipTitle = "증강 미선택";
    public static readonly string TowerUpgradeSkipped = "타워 증강을 하지 않고\n스킵하시겠습니까?";
    public static readonly string TowerUpgradeDeleted = "(스킵하면 타워 증강이 사라집니다.)";
    public static readonly string DeleteTowerConfirm = "를\n정말 제거하시겠습니까?";

    public static readonly string TowerSettingPopupTitle = "타워 정보";
    public static readonly string TowerUpgradePopupTitle = "타워 선택";
    public static readonly string Confirm = "확인";
    public static readonly string Choose = "선택";
}
