using UnityEngine;

[CreateAssetMenu(fileName = "TowerDataSO", menuName = "Scriptable Objects/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public TargetRangeSO rangeData;
    public BaseTargetPriority targetPriority;

    public string towerId;
}