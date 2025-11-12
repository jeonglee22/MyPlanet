using Unity.VisualScripting;
using UnityEngine;

public static class TagName
{
    public static readonly string Planet = "Planet";
    public static readonly string Projectile = "Projectile";
    public static readonly string DropItem = "DropItem";
}

public enum Languages
{
    Korean,
    English,
    Japanese,
}

public static class DataTableIds
{
    public static readonly string[] StringTableIds =
    {
        "StringTableKr",
        "StringTableEn",
        "StringTableJp",
    };

    public static string String => StringTableIds[(int)Variables.Language]; //언어 선택 / 프로퍼티로 바꾼 이유 : DataTableManager에서 고정된 id와 고정되지않은 id를 구분하기 위해서다.
    public static readonly string Item = "ItemTable";
}

public static class Variables
{
    public static Languages Language = Languages.Korean;
}
