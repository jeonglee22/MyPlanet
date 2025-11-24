using UnityEngine;

public enum FireType
{
    Projectile = 0,
}

[CreateAssetMenu(fileName = "TowerDataSO", menuName = "Scriptable Objects/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public int towerIdInt;          // AttackTower_Id
    public string towerId;          // AttackTowerName
    public FireType fireType;       // FireType (0,1,2)
    public float fireRate = 1f;     // AttackSpeed
    public TargetRangeSO rangeData; // AttackRange → range
    public float Accuracy = 90f;   // Accuracy (float)
    public float grouping = 0f;   // grouping (float)
    public int projectileCount = 1; // ProjectileNum
    public int projectileIdFromTable;      // 🔹 Projectile_ID
    public ProjectileData projectileType;  // ProjectileData In RunTime 
    public int randomAbilityGroupId;      // RandomAbilityGroup_ID

    public BaseTargetPriority targetPriority;
}