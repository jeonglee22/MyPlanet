using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    private TowerTargetingSystem targetingSystem;
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

    //not yet_20251117 13:52 #117
    private float hitRadiusBuffMul=1f; //hitbox Size, Mul or Add?
    private float percentPenetrationBuffMul=1f;
    private float fixedPenetrationBuffAdd=0f;
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

    //projectile Count---------------------------------------
    private int baseProjectileCount = 1; //from TargetDataSO (NOT Data Table)
    private int projectileCountBuffAdd = 0; 
    public int CurrentProjectileCount
    {
        get
        {
            int finalCount = baseProjectileCount + projectileCountBuffAdd;
            return Mathf.Max(1, finalCount);
        }
    }
    //-------------------------------------------------------

    //Apply Buff Version Projectile Data SO------------------
    private int projectileId = 1100003; //test
    private ProjectilePoolManager projectilePoolManager;
    private ProjectileData currentProjectileData; //base projectile data from Data Table
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

        projectilePoolManager = GameObject
            .FindGameObjectWithTag("ProjectilePoolManager")
            .GetComponent<ProjectilePoolManager>();
    }

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        //Connect Data Table To baseProjectileData(=currentProjectileData) 
        currentProjectileData = DataTableManager.ProjectileTable.Get(projectileId);
        //Connect Tower Data
        towerData.projectileType = currentProjectileData;
        //Set Projectile Count -> From Tower Data SO (NOT Data Table)
        baseProjectileCount = Mathf.Max(1, towerData.projectileCount);
    }

    private void Update()
    {
        if (towerData == null || targetingSystem == null) return;

        shootTimer += Time.deltaTime;

        //Fire Rate Buff Calculator
        float finalFireRate = CurrentFireRate;
        if (finalFireRate <= 0) return;
        float shootInterval = 1f / finalFireRate;

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

        //Buffed Tower Data
        ProjectileData buffedData = CurrentProjectileData; //Buffed Data
        if (buffedData == null) return;

        //Enemy Debug---------------------------------------
        var targetEnemy = target as Enemy;
        string strategyName = targetingSystem.GetTowerData()?.targetPriority?.GetType().Name ?? "Unknown";
        // Debug.Log($"[SHOOT] Tower: {gameObject.name} | Strategy: {strategyName} | Target: {(target as MonoBehaviour)?.name} | HP: {target.maxHp} | ATK: {target.atk} | DEF: {target.def} | Distance: {Vector3.Distance(transform.position, target.position):F2}");
        //--------------------------------------------

        int shotCount = CurrentProjectileCount; //From TowerDataSO (NO DataTable)
        var baseData = towerData.projectileType; //Pooling Key

        //add projectile debug-----------------------
        float spreadAngle = 10f;
        float centerIndex = (shotCount - 1) * 0.5f;
        //-------------------------------------------

        for (int i = 0; i < shotCount; i++)
        {
            //add projectile debug-------------------
            float offsetIndex = i - centerIndex;
            Quaternion spreadRot = Quaternion.AngleAxis(offsetIndex * spreadAngle, Vector3.up);
            Vector3 shotDir = spreadRot * direction;
            //Vector3 shotDir = direction; 
            //---------------------------------------

            //Pool (Using BaseData For Recycle)
            var projectile = ProjectilePoolManager.Instance.GetProjectile(baseData);
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(shotDir);

            //Initialize Buffed Data
            projectile.Initialize(
                buffedData,
                baseData,
                shotDir,
                true,
                projectilePoolManager.ProjectilePool
                );

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

    //If You Need Method------------------------------------------------
    public void Shoot(Vector3 direction, bool IsHit)
    {
        if (towerData == null || towerData.projectileType == null) return;

        var baseData = currentProjectileData;
        var buffedData = CurrentProjectileData;
        currentProjectileData = baseData;

        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);
        projectile.Initialize(buffedData,baseData, direction, IsHit, projectilePoolManager.ProjectilePool);
        
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
        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);
        projectile.Initialize(CurrentProjectileData, currentProjectileData, direction, IsHit, projectilePoolManager.ProjectilePool);
    }

    private void FastShoot(Vector3 direction, bool IsHit)
    {
        Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.LookRotation(direction);
        projectile.Initialize(CurrentProjectileData, currentProjectileData, direction, IsHit, projectilePoolManager.ProjectilePool);
        projectile.GetComponent<Projectile>().totalSpeed += 20f;
    }

    private void DoubleShoot(Vector3 direction, bool IsHit)
    {
        for(int i = 0; i < 2; i++)
        {
            Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(currentProjectileData);
            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(direction);
            projectile.Initialize(CurrentProjectileData, currentProjectileData, direction + new Vector3(1,0,0) * ((0.5f - i) * 2f), IsHit, projectilePoolManager.ProjectilePool);
        }
    }

    public void SetUpBuff(AmplifierTowerDataSO amp) //Set up Buff this Tower 
    {
        if (amp == null) //Consider remove buffed
        {
            damageBuffMul = 1f;
            fireRateBuffMul = 1f;
            accelerationBuffAdd = 0f;
            projectileCountBuffAdd = 0;

            //not YET(2025-11-19 19:07)
            hitRadiusBuffMul = 1f;
            percentPenetrationBuffMul = 1f;
            fixedPenetrationBuffAdd = 0f;
            targetNumberBuffAdd = 0;
            hitRateBuffMul = 1f;

            return;
        }
        else
        {
            damageBuffMul += amp.DamageBuff;
            fireRateBuffMul *= amp.FireRateBuff;
            accelerationBuffAdd *= amp.AccelerationBuff;
            projectileCountBuffAdd += amp.ProjectileCountBuff;

            //not yet_20251119 19:07
            hitRadiusBuffMul += amp.HitRadiusBuff;
            percentPenetrationBuffMul = amp.PercentPenetrationBuff;
            fixedPenetrationBuffAdd = amp.FixedPenetrationBuff;
            targetNumberBuffAdd += amp.TargetNumberBuff;
            hitRateBuffMul *= amp.HitRateBuff;
        }
    }

    private ProjectileData GetBuffedProjectileData() //making runtime once
    {
        if (currentProjectileData==null) return null;

        var baseData = currentProjectileData;

        if (addBuffProjectileData==null)
        {
            //addBuffProjectileData = ScriptableObject.CreateInstance<ProjectileData>();
            var data = new ProjectileData();
            addBuffProjectileData = data;
        }

        //Base Projectile Data (Not YET_ 20251117 14:38)------------------------
        addBuffProjectileData.Projectile_ID = baseData.Projectile_ID;
        addBuffProjectileData.ProjectileName = baseData.ProjectileName;
        addBuffProjectileData.Attack = baseData.Attack;
        addBuffProjectileData.AttackType = baseData.AttackType;
        addBuffProjectileData.RemainTime = baseData.RemainTime;


        float finalTargetNumber = baseData.TargetNum + targetNumberBuffAdd;
        addBuffProjectileData.TargetNum = Mathf.Max(1, finalTargetNumber);

        addBuffProjectileData.CollisionSize = baseData.CollisionSize;
        addBuffProjectileData.FixedPenetration = baseData.FixedPenetration;
        addBuffProjectileData.RatePenetration = baseData.RatePenetration;
        addBuffProjectileData.ProjectileSpeed = baseData.ProjectileSpeed;
        addBuffProjectileData.ProjectileAddSpeed = baseData.ProjectileAddSpeed;
        addBuffProjectileData.HitType = baseData.HitType;
        addBuffProjectileData.ProjectileProperties1_ID = baseData.ProjectileProperties1_ID;
        addBuffProjectileData.ProjectileProperties1Value = baseData.ProjectileProperties1Value;
        addBuffProjectileData.ProjectileProperties2_ID = baseData.ProjectileProperties2_ID;
        addBuffProjectileData.ProjectileProperties2Value = baseData.ProjectileProperties2Value;
        addBuffProjectileData.ProjectileProperties3_ID = baseData.ProjectileProperties3_ID;
        addBuffProjectileData.ProjectileProperties3Value = baseData.ProjectileProperties3Value;
        //---------------------------------------------

        //Buffed Projectile Data ----------------------
        //(Damage, Acceleration)
        addBuffProjectileData.Attack = baseData.Attack * damageBuffMul;
        addBuffProjectileData.ProjectileAddSpeed = baseData.ProjectileAddSpeed + accelerationBuffAdd;
        //---------------------------------------------
        return addBuffProjectileData;
    }
}