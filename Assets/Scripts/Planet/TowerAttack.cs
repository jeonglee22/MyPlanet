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
    public TowerDataSO AttackTowerData => towerData;
    private float shootTimer;

    private List<int> abilities;
    public List<int> Abilities => abilities;

    //test
    private ProjectilePoolManager projectilePoolManager;
    //------------ Amplifier Buff Field ---------------------
    private float damageBuffMul=1f; //damage = baseDamage * damageBuffMul
    public float fireRateBuffMul=1f; //fireRate = baseFireRate * fireRateBuffMul
    private float accelerationBuffAdd=0f;  //just add

    private float hitRadiusBuffMul=1f; //hitbox Size, Mul or Add?
    private float percentPenetrationBuffMul=1f;
    private float fixedPenetrationBuffAdd=0f;
    private int projectileCountBuffAdd=0; //new variable
    private int targetNumberBuffAdd=0;
    private float hitRateBuffMul=1f;

    public float CurrentFireRate
    {
        get
        {
            if (towerData == null) return 0f;
            return towerData.fireRate * fireRateBuffMul;
        }
    }

    //Apply Buff Version Projectile Data SO
    private ProjectileData addBuffProjectileData; //making runtime once
    public ProjectileData CurrentProjectileData
    {
        get
        {
            var buffed = GetBuffedProjectileData();
            if (buffed != null) return buffed;
            return towerData != null ? towerData.projectileType : null;
        }
    }
    //-------------------------------------------------------


    private void Awake()
    {
        targetingSystem = GetComponent<TowerTargetingSystem>();
        abilities = new List<int>();
        // abilities.Add(AbilityManager.Instance.AbilityDict[0]);
        // SetRandomAbility();

        if (firePoint == null) firePoint = transform;

        projectilePoolManager = GameObject.FindGameObjectWithTag("ProjectilePoolManager").GetComponent<ProjectilePoolManager>();
    }

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        currentProjectileData = data.projectileType;
    }

    private void Update()
    {
        if (towerData == null || targetingSystem == null)
        {
            // Debug.LogWarning($"[TowerAttack.Update] {gameObject.name} | towerData: {(towerData != null ? "OK" : "NULL")} | targetingSystem: {(targetingSystem != null ? "OK" : "NULL")}");
            return;
        }

        shootTimer += Time.deltaTime;

        //Fire Rate Buff Calculator
        float basicFireRate = towerData.fireRate;
        float finalFireRate = basicFireRate * fireRateBuffMul;
        if (finalFireRate <= 0) return;
        float shootInterval = 1f / finalFireRate;

        if(shootTimer>=shootInterval)
        {
            //Debug.Log($"[TowerAttack.Update] {gameObject.name} attempting to shoot | CurrentTarget: {(targetingSystem.CurrentTarget != null ? "EXISTS" : "NULL")}");
            ShootAtTarget();
            shootTimer = 0f;
        }

        Debug.Log($"[ShootTimer] {gameObject.name} finalFireRate={finalFireRate}");

    }

    private void ShootAtTarget()
    {
        var target = targetingSystem.CurrentTarget;
        // Debug.Log($"[ShootAtTarget] {gameObject.name} | Target: {(target != null ? (target as MonoBehaviour)?.name : "NULL")} | isAlive: {(target != null ? target.isAlive.ToString() : "N/A")}");
        if (target == null || !target.isAlive) return;

        Vector3 direction = (target.position - transform.position).normalized;

        //Enemy Debug---------------------------------------
        var targetEnemy = target as Enemy;
        string strategyName = targetingSystem.GetTowerData()?.targetPriority?.GetType().Name ?? "Unknown";
        // Debug.Log($"[SHOOT] Tower: {gameObject.name} | Strategy: {strategyName} | Target: {(target as MonoBehaviour)?.name} | HP: {target.maxHp} | ATK: {target.atk} | DEF: {target.def} | Distance: {Vector3.Distance(transform.position, target.position):F2}");
        //--------------------------------------------

        var projectile = ProjectilePoolManager.Instance.GetProjectile(towerData.projectileType);
        if(projectile==null)
        {
            projectile = Instantiate(
                towerData.projectileType.projectilePrefab, 
                transform.position, 
                Quaternion.LookRotation(direction)
                ).GetComponent<Projectile>();
        }

        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        //Buffed check
        var buffedData = GetBuffedProjectileData();
        if (buffedData == null) return;

        projectile.Initialize(buffedData, direction, true, projectilePoolManager.ProjectilePool);

        foreach (var abilityId in abilities)
        {
            var ability = AbilityManager.Instance.GetAbility(abilityId);
            ability.ApplyAbility(projectile.gameObject);
            projectile.abilityAction += ability.ApplyAbility;
            projectile.abilityRelease += ability.RemoveAbility;
        }
    }

    public void AddAbility(int ability)
    {
        abilities.Add(ability);
    }

    public void SetRandomAbility()
    {
        var ability = AbilityManager.Instance.GetRandomAbility();
        abilities.Add(ability);
        // Debug.Log(ability);
    }

    public void Shoot(Vector3 direction, bool IsHit)
    {
        if (towerData == null || towerData.projectileType == null) return;

        var baseData = towerData.projectileType;
        currentProjectileData = baseData;

        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        if (projectile == null)
        {
            projectile = Instantiate(
                currentProjectileData.projectilePrefab, 
                transform.position, 
                Quaternion.LookRotation(direction)
                ).GetComponent<Projectile>();
        }

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);

        //Buffed Check
        var buffedData = GetBuffedProjectileData();
        if (buffedData == null) return;

        projectile.Initialize(currentProjectileData, direction, IsHit, projectilePoolManager.ProjectilePool);
        
        foreach (var abilityId in abilities)
        {
            var ability = AbilityManager.Instance.GetAbility(abilityId);
            // ability.Setting(projectile.gameObject);
            // ability.ApplyAbility(projectile.gameObject);
            ability.ApplyAbility(projectile.gameObject);
            projectile.abilityAction += ability.ApplyAbility;
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
        projectile.Initialize(currentProjectileData, direction, IsHit, projectilePoolManager.ProjectilePool);
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
        projectile.Initialize(currentProjectileData, direction, IsHit, projectilePoolManager.ProjectilePool);
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

            projectile.Initialize(currentProjectileData, direction + new Vector3(1,0,0) * ((0.5f - i) * 2f), IsHit, projectilePoolManager.ProjectilePool);
        }
    }

    public void SetUpBuff(AmplifierTowerDataSO amp) //Set up Buff this Tower 
    {
        if (amp == null) //Consider remove buffed
        {
            damageBuffMul = 1f;
            fireRateBuffMul = 1f;
            accelerationBuffAdd = 0f;
            //not YET(2025-11-17 14:45)
            hitRadiusBuffMul = 1f;
            percentPenetrationBuffMul = 1f;
            fixedPenetrationBuffAdd = 0f;
            projectileCountBuffAdd = 0;
            targetNumberBuffAdd = 0;
            hitRateBuffMul = 1f;
            return;
        }

        damageBuffMul += amp.DamageBuff;
        fireRateBuffMul *= amp.FireRateBuff;
        accelerationBuffAdd *= amp.AccelerationBuff;

        //not yet_20251117 14:14
        hitRadiusBuffMul = 1f + amp.HitRadiusBuff;
        percentPenetrationBuffMul = amp.PercentPenetrationBuff;
        fixedPenetrationBuffAdd = amp.FixedPenetrationBuff;
        projectileCountBuffAdd = amp.ProjectileCountBuff;
        targetNumberBuffAdd = amp.TargetNumberBuff;
        hitRateBuffMul = amp.HitRateBuff;

        Debug.Log($"[TowerAttack Buff] {name} | dmgMul={damageBuffMul:F2}, fireRateMul={fireRateBuffMul:F2}, accelAdd={accelerationBuffAdd:F2}");
    }

    private ProjectileData GetBuffedProjectileData() //making runtime once
    {
        if (towerData == null || towerData.projectileType == null) return null;

        var baseData = towerData.projectileType;
        if(addBuffProjectileData==null)
        {
            addBuffProjectileData = ScriptableObject.CreateInstance<ProjectileData>();
        }

        //Base Projectile Data (Not YET_ 20251117 14:38)------------------------
        addBuffProjectileData.projectileType = baseData.projectileType;
        addBuffProjectileData.projectilePrefab = baseData.projectilePrefab;
        addBuffProjectileData.hitEffect = baseData.hitEffect;
        addBuffProjectileData.lifeTime = baseData.lifeTime;
        addBuffProjectileData.targetNumber = baseData.targetNumber;
        addBuffProjectileData.hitRadius = baseData.hitRadius;
        addBuffProjectileData.fixedPanetration = baseData.fixedPanetration;
        addBuffProjectileData.percentPenetration = baseData.percentPenetration;
        //---------------------------------------------

        //Buffed Projectile Data ----------------------
        //(Damage, Acceleration)
        addBuffProjectileData.damage = baseData.damage * damageBuffMul;
        addBuffProjectileData.acceleration = baseData.acceleration + accelerationBuffAdd;
        //---------------------------------------------

        return addBuffProjectileData;
    }
}