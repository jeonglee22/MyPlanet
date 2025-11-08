using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject prefab;
    public MovementType movementType;

    public float maxHealth;
    public float speed;
    public float damage;

}

public enum MovementType
{
    Meteor,
    Missile,
}
