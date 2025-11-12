using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public enum AttackAbility
    {
        Basic,
        FastShoot,
        DoubleShoot,
    }


    [SerializeField] private List<ProjectileData> projectileDatas;
    private ProjectileData currentProjectileData;

    [SerializeField] private Projectile projectilePrefab;

    [SerializeField] private AttackAbility ability = AttackAbility.Basic;

    public void Shoot(ProjectileType projectileType, Vector3 direction, bool IsHit)
    {
        switch (projectileType)
        {
            case ProjectileType.Normal:
                currentProjectileData = projectileDatas.Find(p => p.projectileType == ProjectileType.Normal);
                break;
        }

        switch (ability)
        {
            case AttackAbility.Basic:
                BasicShoot(direction, IsHit);
                break;
            case AttackAbility.FastShoot:
                FastShoot(direction, IsHit);
                break;
            case AttackAbility.DoubleShoot:
                DoubleShoot(direction, IsHit);
                break;
        }
    }

    private void BasicShoot(Vector3 direction, bool IsHit)
    {
        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        if (projectile == null)
        {
            return;
        }

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        projectile.Initialize(currentProjectileData, direction, IsHit);
    }

    private void FastShoot(Vector3 direction, bool IsHit)
    {
        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        if (projectile == null)
        {
            return;
        }

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        projectile.Initialize(currentProjectileData, direction, IsHit);

        projectile.GetComponent<Projectile>().totalSpeed += 20f;
    }

    private void DoubleShoot(Vector3 direction, bool IsHit)
    {
        for(int i = 0; i < 2; i++)
        {
            Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
            if (projectile == null)
            {
                return;
            }

            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            projectile.Initialize(currentProjectileData, direction + new Vector3(1,0,0) * ((0.5f - i) * 2f), IsHit);
        }
    }
}
