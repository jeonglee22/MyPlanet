using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    private TowerDataSO towerData;

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
    }

    public void Shoot(ProjectileType projectileType, Vector3 direction, bool IsHit)
    {
        if(towerData==null||towerData.projectileType==null)
        {
            UnityEngine.Debug.Log($"Not Find TowerData ProjectileData{gameObject.name}");
            return;
        }

        ProjectileData currentProjectileData = towerData.projectileType;

        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        if (projectile == null)
        {
            projectile = Instantiate(currentProjectileData.projectilePrefab, transform.position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
        }

        projectile.Initialize(currentProjectileData, direction, IsHit);
    }
}