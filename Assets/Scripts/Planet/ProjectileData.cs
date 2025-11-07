using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Data", menuName = "Planet/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    public ProjectileType projectileType;
    public GameObject projectilePrefab;
    public ParticleSystem hitEffect;
    public float damage = 10f;
    public float fixedPanetration = 5f;
    public float percentPenetration = 0f;
    public float speed = 5f;
    public float acceleration = 0f;
    public int targetNumber = 1;
    public float lifeTime = 10f;
    public float hitRadius = 10f;

}

public enum ProjectileType
{
    Normal,
    Piercing,
    Explosive,
    Chain,
    Homing,
}
