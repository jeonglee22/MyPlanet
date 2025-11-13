using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    private TowerTargetingSystem targetingSystem;
    private ProjectileData currentProjectileData;
    private TowerDataSO towerData;
    private float shootTimer;

    private List<IAbility> abilities;

    private void Awake()
    {
        targetingSystem = GetComponent<TowerTargetingSystem>();
        abilities = new List<IAbility>();
        // abilities.Add(AbilityManager.Instance.AbilityDict[0]);
        SetRandomAbility();

        if (firePoint == null) firePoint = transform;
    }

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        currentProjectileData = data.projectileType;
    }

    private void Update()
    {
        if (towerData == null) return;

        shootTimer += Time.deltaTime;
        float shootInterval = 1f / towerData.fireRate;

        if(shootTimer>=shootInterval)
        {
            ShootAtTarget();
            shootTimer = 0f;
        }
    }

    private void ShootAtTarget()
    {
        var target = targetingSystem.CurrentTarget;
        if (target == null || !target.isAlive) return;

        Vector3 direction = (target.position - transform.position).normalized;

        Debug.Log($"{name} shooting at {target} | Direction: {direction} | Projectile: {towerData.projectileType?.projectilePrefab.name}");

        var projectile = ProjectilePoolManager.Instance.GetProjectile(towerData.projectileType);
        if(projectile==null)
        {
            projectile = Instantiate(towerData.projectileType.projectilePrefab, transform.position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
        }

        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);
        projectile.Initialize(towerData.projectileType, direction, true);

        foreach (var ability in abilities)
        {
            ability.ApplyAbility(projectile.gameObject);
            projectile.abilityRelease += ability.RemoveAbility;
        }
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
        if (towerData == null || towerData.projectileType == null) return;

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