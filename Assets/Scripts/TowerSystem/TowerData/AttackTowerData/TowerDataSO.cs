using UnityEngine;

public enum FireType
{
    Projectile = 0,
}

[System.Serializable]
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
    
    [Header("Visual(Except Laser)")]
    public GameObject projectilePrefab;

    [Header("Audio")]
    public AudioClip shootSfx;      
    public AudioClip laserLoopSfx;             
    [Range(0f, 1f)] public float shootVolume = 1f;
    [Range(0f, 1f)] public float laserLoopVolume = 1f;
    public Vector2 shootPitchRange = new Vector2(1f, 1f);
}