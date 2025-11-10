using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TowerAttack : MonoBehaviour
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

        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        if (projectile == null)
        {
            return;
        }

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        projectile.Initialize(currentProjectileData, direction, IsHit);
        
        
    }
}
