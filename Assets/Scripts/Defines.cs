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
}

public static class ObjectName
{
    public static readonly string PatternProjectile = "PatternProjectile";
    public static readonly string Enemy = "EnemySample";
    public static readonly string ProjectilePrefab = "Projectile_Sample";
    public static readonly string Projectile = "Projectile";
}

public enum Languages
{
    Korean,
    English,
    Japanese,
}

public enum PrefabType
{
    Enemy,
    PatternProjectile
}

public enum ProjectileType
{
    Normal,
}

public static class DataTableIds
{
    public static readonly string[] StringTableIds =
    {
        "StringTableKr",
        "StringTableEn",
        "StringTableJp",
    };

    public static string String => StringTableIds[(int)Variables.Language];
    public static readonly string Item = "ItemTable";
    public static readonly string Enemy = "EnemyTable";
    public static readonly string Combine = "CombineTable";
    public static readonly string Wave = "WaveTable";
    public static readonly string Projectile = "ProjectileTable";
}

public static class Variables
{
    public static Languages Language = Languages.Korean;

    public static int Stage {get; set;} = 1;
}
