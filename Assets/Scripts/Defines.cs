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
}

public static class ObjectName
{
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string Enemy = "EnemySample";
    public static readonly string ProjectilePrefab = "Projectile_Sample";
    public static readonly string Projectile = "Projectile";
    public static readonly string ChainEffect = "ChainEffect";
    public static readonly string MeteorChild = "MeteorChild";
}

public static class SceneName
{
    public static readonly string LoadingScene = "LoadingScene";
    public static readonly string LoginScene = "LoginScene";
    public static readonly string BattleScene = "BattleScene";
    public static readonly string EnemyTestScene = "EnemyTestScene";
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
}

public enum MoveType
{
    StraightDown,
    Homing,
    Chase,
    FollowParent,
    DescendAndStopMovement = 10,
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
    SaturnMeteorCircleSummon,
    TitanRevolution,
    HomingMeteorCluster,
    ChaseMeteorCluster
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
}

public static class Variables
{
    public static int Stage {get; set;} = 1;
}

public static class AddressLabel
{
    public static readonly string Prefab = "Prefab";
    public static readonly string PoolObject = "PoolObject";
    public static readonly string PatternProjectile = "PatternProjectile";
}
