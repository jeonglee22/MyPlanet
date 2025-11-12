using UnityEngine;

[CreateAssetMenu(fileName = "TowerDataSO", menuName = "Scriptable Objects/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public TargetRangeSO rangeData;
    public BaseTargetPriority targetPriority;
    public ProjectileData projectileType;

    public string towerId;

    [Header("Accuracy & Hit Settings")]
    public float hitRate = 100f;
    public float spreadAccuracy = 100f;
}