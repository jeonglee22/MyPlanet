using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileOffset = 0.05f;
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
    public float DamageBuffMMul { get { return damageBuffMul; } set { damageBuffMul = value; } }
    private float accelerationBuffAdd=0f;  //just add
    public float fireRateBuffMul = 1f; //fireRate = baseFireRate * fireRateBuffMul
    public float CurrentFireRate
    {
        get
        {
            if (towerData == null) return 0f;
            return towerData.fireRate * fireRateBuffMul;
        }
    }

    private int newProjectileAttackType;
    public int NewProjectileAttackType { get { return newProjectileAttackType; } set { newProjectileAttackType = value; } }

    private float hitRadiusBuffMul = 1f; //hitbox Size, Mul or Add?
    public float HitRadiusBuffMul { get { return hitRadiusBuffMul; } set { hitRadiusBuffMul = value; } }
    private float percentPenetrationBuffMul = 1f;
    public float PercentPenetrationBuffMul { get { return percentPenetrationBuffMul; } set { percentPenetrationBuffMul = value; } }
    private float fixedPenetrationBuffAdd = 0f;
    public float FixedPenetrationBuffAdd { get { return fixedPenetrationBuffAdd; } set { fixedPenetrationBuffAdd = value; } }
    private int targetNumberBuffAdd = 0;
    public int TargetNumberBuffAdd { get { return targetNumberBuffAdd; } set { targetNumberBuffAdd = value; } }
    private float hitRateBuffMul = 1f;

    public float BasicFireRate => towerData.fireRate;
    public float BasicHitRate => towerData.Accuracy;
    public float FinalHitRate => towerData.Accuracy * hitRadiusBuffMul;
    public float HitRateBuffMultiplier => fireRateBuffMul;


    //projectile Count---------------------------------------
    private int baseProjectileCount = 1; //from TargetDataSO (NOT Data Table)
    private int projectileCountBuffAdd = 0; 
    public int ProjectileCountBuffAdd { get { return projectileCountBuffAdd; } set { projectileCountBuffAdd = value; } }
    public int CurrentProjectileCount
    {
        get
        {
            int finalCount = baseProjectileCount + projectileCountBuffAdd;
            return Mathf.Max(1, finalCount);
        }
    }
    public int BaseProjectileCount => baseProjectileCount;
    public int FinalProjectileCount => CurrentProjectileCount;
    //-------------------------------------------------------

    //Apply Buff Version Projectile Data SO------------------
    private ProjectileData currentProjectileData; //base projectile data from Data Table
    private ProjectileData addBuffProjectileData; //making runtime once
    public ProjectileData BaseProjectileData => currentProjectileData;
    public ProjectileData BuffedProjectileData => CurrentProjectileData;
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
            .FindGameObjectWithTag(TagName.ProjectilePoolManager)
            .GetComponent<ProjectilePoolManager>();
    }

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        if (towerData == null) return;
        //Connect Data Table To baseProjectileData(=currentProjectileData) 
        currentProjectileData = DataTableManager.ProjectileTable.Get(towerData.projectileIdFromTable);
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
        // float spreadAngle = 10f;
        float centerIndex = (shotCount - 1) * 0.5f;
        //-------------------------------------------

        for (int i = 0; i < shotCount; i++)
        {
            //add projectile debug-------------------
            float offsetIndex = i - centerIndex;
            // Quaternion spreadRot = Quaternion.AngleAxis(offsetIndex * spreadAngle, Vector3.up);
            // Vector3 shotDir = spreadRot * direction;
            //Vector3 shotDir = direction; 
            //---------------------------------------

            //Pool (Using BaseData For Recycle)
            var projectile = ProjectilePoolManager.Instance.GetProjectile(baseData);

            var verticalDirection = new Vector3(-direction.y, direction.x, direction.z).normalized;

            projectile.transform.position = firePoint.position + verticalDirection * projectileOffset * offsetIndex;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            //Initialize Buffed Data
            projectile.Initialize(
                buffedData,
                baseData,
                direction,
                true,
                projectilePoolManager.ProjectilePool
                );

            if(CurrentProjectileData.AttackType == (int)ProjectileType.Homing)
            {
                //Set Target for Homing
                projectile.SetHomingTarget(target);
            }

            foreach (var abilityId in abilities)
            {
                var ability = AbilityManager.GetAbility(abilityId);
                ability.ApplyAbility(projectile.gameObject);
                projectile.abilityAction += ability.ApplyAbility;
                projectile.abilityRelease += ability.RemoveAbility;
                ability.Setting(gameObject);
            }
        }
    }

    public void AddAbility(int ability)
    {
        abilities.Add(ability);
    }

    public void SetRandomAbility()
    {
        var ability = AbilityManager.GetRandomAbility();
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
            var ability = AbilityManager.GetAbility(abilityId);
            // ability.Setting(projectile.gameObject);
            // ability.ApplyAbility(projectile.gameObject);
            ability.ApplyAbility(projectile.gameObject);
            projectile.abilityAction += ability.ApplyAbility;
            projectile.abilityRelease += ability.RemoveAbility;
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

        addBuffProjectileData = currentProjectileData.Clone();

        //Base Projectile Data (Not YET_ 20251117 14:38)------------------------
        float finalTargetNumber = currentProjectileData.TargetNum + targetNumberBuffAdd;
        addBuffProjectileData.TargetNum = Mathf.Max(1, finalTargetNumber);
        //---------------------------------------------
        addBuffProjectileData.CollisionSize = currentProjectileData.CollisionSize * hitRadiusBuffMul;

        //Penetration Buff
        addBuffProjectileData.FixedPenetration = currentProjectileData.FixedPenetration + fixedPenetrationBuffAdd;
        addBuffProjectileData.RatePenetration = currentProjectileData.RatePenetration * percentPenetrationBuffMul;

        //Buffed Projectile Data ----------------------
        //(Damage, Acceleration)
        addBuffProjectileData.Attack = currentProjectileData.Attack * damageBuffMul;
        addBuffProjectileData.ProjectileAddSpeed = currentProjectileData.ProjectileAddSpeed + accelerationBuffAdd;
        addBuffProjectileData.AttackType = currentProjectileData.AttackType == newProjectileAttackType ? currentProjectileData.AttackType : newProjectileAttackType;
        currentProjectileData.AttackType = addBuffProjectileData.AttackType;
        //---------------------------------------------
        return addBuffProjectileData;
    }
}