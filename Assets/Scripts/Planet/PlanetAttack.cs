using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlanetAttack : MonoBehaviour
{
    [SerializeField] private List<ProjectileData> projectileDatas;
    private ProjectileData currentProjectileData;

    [SerializeField] private Projectile projectilePrefab;

    private void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Alpha1) || Input.touchCount != 0)
        {
            Shoot(ProjectileType.Normal, transform.forward, true);
        }
        #endif
    }

    public void Shoot(ProjectileType projectileType, Vector3 direction, bool IsHit)
    {
        switch (projectileType)
        {
            case ProjectileType.Normal:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Normal);
                break;
            case ProjectileType.Piercing:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Piercing);
                break;
            case ProjectileType.Explosive:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Explosive);
                break;
            case ProjectileType.Chain:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Chain);
                break;
            case ProjectileType.Homing:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Homing);
                break;
        }

        Projectile newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        newProjectile.Initialize(currentProjectileData, direction, IsHit);
    }
}
