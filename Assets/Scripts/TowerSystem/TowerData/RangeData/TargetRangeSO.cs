using UnityEngine;

public enum RangeType
{
    Short,
    Mid,
    Long
}

[CreateAssetMenu(fileName = "TargetRangeSO", menuName = "Scriptable Objects/TargetRangeSO")]
public class TargetRangeSO : ScriptableObject
{
    [SerializeField] RangeType rangeType=RangeType.Mid;

    [SerializeField] private float shortRange = 5f;
    [SerializeField] private float midRange = 10f;
    [SerializeField] private float longRange = 15f;

    public RangeType RangeType => rangeType;

    public float GetRange()
    {
        return rangeType switch
        {
            RangeType.Short=>shortRange,
            RangeType.Mid=>midRange,
            RangeType.Long=>longRange,
            _=>midRange
        };
    }
}
