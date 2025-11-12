using System;
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

    private ProjectileData currentProjectileData;
    private TowerDataSO towerData;

    [SerializeField] private AttackAbility ability = AttackAbility.Basic;

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        currentProjectileData = data.projectileType;
    }

    public void Shoot(Vector3 direction, bool IsHit)
    {
        if(towerData==null||towerData.projectileType==null)
        {
            UnityEngine.Debug.Log($"Not Find TowerData ProjectileData{gameObject.name}");
            return;
        }

        currentProjectileData = towerData.projectileType;

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
            projectile = Instantiate(currentProjectileData.projectilePrefab, transform.position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
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
            projectile = Instantiate(currentProjectileData.projectilePrefab, transform.position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
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
                projectile = Instantiate(currentProjectileData.projectilePrefab, transform.position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
            }

            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            projectile.Initialize(currentProjectileData, direction + new Vector3(1,0,0) * ((0.5f - i) * 2f), IsHit);
        }
    }
}