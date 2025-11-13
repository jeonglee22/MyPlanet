using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    private ProjectileData currentProjectileData;
    private TowerDataSO towerData;

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        currentProjectileData = data.projectileType;
    }

    private List<IAbility> abilities;

    private void Awake()
    {
        abilities = new List<IAbility>();
        // abilities.Add(AbilityManager.Instance.AbilityDict[0]);
        SetRandomAbility();
    }
    
    public void AddAbility(IAbility ability)
    {
        abilities.Add(ability);
    }

    public void SetRandomAbility()
    {
        var ability = AbilityManager.Instance.GetRandomAbility();
        abilities.Add(ability);
        Debug.Log(ability);
    }

    public void Shoot(Vector3 direction, bool IsHit)
    {
        if(towerData==null||towerData.projectileType==null)
        {
            UnityEngine.Debug.Log($"Not Find TowerData ProjectileData{gameObject.name}");
            return;
        }

        currentProjectileData = towerData.projectileType;

        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        if (projectile == null)
        {
            projectile = Instantiate(currentProjectileData.projectilePrefab, transform.position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
        }

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        projectile.Initialize(currentProjectileData, direction, IsHit);
        
        foreach (var ability in abilities)
        {
            // ability.Setting(projectile.gameObject);
            // ability.ApplyAbility(projectile.gameObject);
            ability.ApplyAbility(projectile.gameObject);
            projectile.abilityRelease += ability.RemoveAbility;
        }

        // switch (attackAbility)
        // {
        //     case AttackAbility.Basic:
        //         BasicShoot(direction, IsHit);
        //         break;
        //     case AttackAbility.FastShoot:
        //         FastShoot(direction, IsHit);
        //         break;
        //     case AttackAbility.DoubleShoot:
        //         DoubleShoot(direction, IsHit);
        //         break;
        // }
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