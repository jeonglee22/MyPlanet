using System;
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
}

public static class ObjectName
{
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string Enemy = "EnemySample";
    public static readonly string ProjectilePrefab = "Projectile_Sample";
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
}

public enum MoveType
{
    StraightDown,
    Homing,
    Chase,
    FollowParent,
    DescendAndStopMovement = 10,
    Revolution = 11,
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
    ChaseMeteorCluster
}

public static class DataTableIds
{
    public static readonly string Item = "ItemTable";
    public static readonly string Enemy = "EnemyTableTest";
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
}

public static class Variables
{
    public static int Stage {get; set;} = 3;
    private static int quasar = 0;
    public static int Quasar
    {
        get => quasar;
        set
        {
            quasar = value;
            if(quasar > 0)
            {
                OnQuasarChanged?.Invoke();
            }
        }
    }
    public static event Action OnQuasarChanged;
    public static Enemy LastBossEnemy {get; set;} = null;
    public static Enemy MiddleBossEnemy {get; set;} = null;
    public static GameObject TestBossEnemyObject {get; set;} = null;
}

public static class AddressLabel
{
    public static readonly string Prefab = "Prefab";
    public static readonly string PoolObject = "PoolObject";
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string Lazer = "Lazer";
    public static readonly string EnemyLazer = "EnemyLazer";
}