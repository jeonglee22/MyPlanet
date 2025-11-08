using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject prefab;
    public MovementType movementType;
    public AbilityType[] abilityTypes;

    public float maxHealth;
    public float speed;
    public float damage;

}

public enum MovementType
{
    StraightDown,
    TargetDirection,
}

public enum AbilityType
{
    None,
    SplitOnDeath,
}
