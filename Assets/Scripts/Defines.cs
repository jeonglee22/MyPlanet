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
}

public enum MoveType
{
    StraightDown,
    Homing,
    Chase,
    FollowParent,
    DescendAndStopMovement = 10,
    Revolution = 11,
    Side,
    TwoPhaseHomingMovement,
    TwoPhaseDownMovement,
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
    HealthPlanet = 0,
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
    public static readonly string TowerUpgrade = "TowerUpgradeTable";
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
        planetId = 300001;
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
}

public static class DatabaseRef
{
    public static readonly string UserProfiles = "users";
    public static readonly string UserPlanets = "userplanets";
    public static readonly string UserTowers = "usertowers";
    public static readonly string UserAttackPowers = "userattackpowers";
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