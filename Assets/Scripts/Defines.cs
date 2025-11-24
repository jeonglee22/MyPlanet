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
}

public enum PatternIds
{
    None = 0,
    MeteorCluster = 1001,
    SimpleShot = 2001,
}

public static class DataTableIds
{
    public static readonly string Item = "ItemTable";
    public static readonly string Enemy = "EnemyTable";
    public static readonly string Combine = "CombineTable";
    public static readonly string Wave = "WaveTable";
    public static readonly string Projectile = "ProjectileTable";
    public static readonly string RandomAbility = "RandomAbilityTable";
    public static readonly string RandomAbilityGroup = "RandomAbilityGroupTable";
}

public static class Variables
{
    public static int Stage {get; set;} = 1;
}
