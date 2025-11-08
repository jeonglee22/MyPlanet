using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlanetAttack : MonoBehaviour
{
    [SerializeField] private List<ProjectileData> projectileDatas;
    private ProjectileData currentProjectileData;

    [SerializeField] private Projectile projectilePrefab;

    public void Shoot(ProjectileType projectileType, Vector3 direction, bool IsHit)
    {
        switch (projectileType)
        {
            case ProjectileType.Normal:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Normal);
                break;
        }

        Projectile newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        newProjectile.Initialize(currentProjectileData, direction, IsHit);
    }
}
