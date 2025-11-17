using UnityEngine;

[CreateAssetMenu(fileName = "TowerDataSO", menuName = "Scriptable Objects/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public TargetRangeSO rangeData;
    public BaseTargetPriority targetPriority;
    public ProjectileData projectileType;

    public string towerId;
    public float fireRate = 1f;
    public int projectileCount=1;

    [Header("Accuracy & Hit Settings")]
    public float hitRate = 100f;
    public float spreadAccuracy = 100f;
}