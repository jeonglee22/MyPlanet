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
    private float accelerationBuffAdd=0f;  // +=
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

    private float hitRadiusBuffMul = 1f; // +=
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
    public float HitRateBuffMultiplier => hitRateBuffMul;


    //projectile Count---------------------------------------
    private int baseProjectileCount = 1; //Only Var
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
        //Set Projectile Count -> From Tower Data SO (NOT Data Table) -> check
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
        Camera cam = Camera.main;
        if (cam == null) return;

        // Buffed Tower Data
        ProjectileData buffedData = CurrentProjectileData; // Buffed Data
        if (buffedData == null) return;

        // Projectile Count
        int shotCount = CurrentProjectileCount; // From TowerDataSO
        var baseData = towerData.projectileType; // Pooling Key
        if (baseData == null) return;

        var attackType = buffedData.AttackType;

        var targets = targetingSystem.CurrentTargets;

        if (targets == null || targets.Count == 0)
        {
            var single = targetingSystem.CurrentTarget;
            FireToTarget(single, buffedData, baseData, shotCount, attackType, cam);
            return;
        }

        foreach (var target in targets)
        {
            FireToTarget(target, buffedData, baseData, shotCount, attackType, cam);
        }
    }

    private void FireToTarget(
        ITargetable target,
        ProjectileData buffedData,
        ProjectileData baseData,
        int shotCount,
        float attackType,
        Camera cam)
    {
        if (target == null || !target.isAlive) return;

        Vector3 vp = cam.WorldToViewportPoint(target.position);
        bool inViewport = (vp.z > 0f &&
                           vp.x >= 0f && vp.x <= 1f &&
                           vp.y >= 0f && vp.y <= 1f);
        if (!inViewport) return;

        Vector3 direction = (target.position - firePoint.position).normalized;
        float centerIndex = (shotCount - 1) * 0.5f;

        for (int i = 0; i < shotCount; i++)
        {
            float offsetIndex = i - centerIndex;
            var projectile = ProjectilePoolManager.Instance.GetProjectile(baseData);
            var verticalDirection = new Vector3(-direction.y, direction.x, direction.z).normalized;

            projectile.transform.position =
                firePoint.position + verticalDirection * projectileOffset * offsetIndex;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            projectile.Initialize(
                buffedData,
                baseData,
                direction,
                true,
                projectilePoolManager.ProjectilePool
            );

            if (attackType == (int)ProjectileType.Homing)
            {
                projectile.SetHomingTarget(target);
            }

            foreach (var abilityId in abilities)
            {
                var ability = AbilityManager.GetAbility(abilityId);
                if (ability == null) continue;
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
        AbilityManager.GetAbility(ability)?.ApplyAbility(gameObject);
        AbilityManager.GetAbility(ability)?.Setting(gameObject);
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
            if (ability == null) continue;
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

            hitRadiusBuffMul = 1f;
            percentPenetrationBuffMul = 1f;
            fixedPenetrationBuffAdd = 0f;
            targetNumberBuffAdd = 0;
            hitRateBuffMul = 1f;

            if (targetingSystem != null) targetingSystem.SetMaxTargetCount(1);

            return;
        }
        //Accumulate Buff Data
        damageBuffMul += amp.DamageBuff;
        fireRateBuffMul *= amp.FireRateBuff;
        accelerationBuffAdd += amp.AccelerationBuff;
        projectileCountBuffAdd += amp.ProjectileCountBuff;
        hitRadiusBuffMul += amp.HitRadiusBuff;
        percentPenetrationBuffMul *= amp.PercentPenetrationBuff;
        fixedPenetrationBuffAdd += amp.FixedPenetrationBuff;
        targetNumberBuffAdd += amp.TargetNumberBuff;
        hitRateBuffMul *= amp.HitRateBuff;

        if (targetingSystem != null)
        {
            int finalTargetCount = 1 + targetNumberBuffAdd;
            targetingSystem.SetMaxTargetCount(finalTargetCount);
        }
    }

    private ProjectileData GetBuffedProjectileData() //making runtime once
    {
        if (currentProjectileData == null) return null;

        addBuffProjectileData = currentProjectileData.Clone();

        float finalTargetNumber = currentProjectileData.TargetNum + targetNumberBuffAdd;
        addBuffProjectileData.TargetNum = Mathf.Max(1, finalTargetNumber);

        addBuffProjectileData.CollisionSize = currentProjectileData.CollisionSize * hitRadiusBuffMul;

        addBuffProjectileData.FixedPenetration = currentProjectileData.FixedPenetration + fixedPenetrationBuffAdd;
        addBuffProjectileData.RatePenetration = currentProjectileData.RatePenetration * percentPenetrationBuffMul;

        addBuffProjectileData.Attack = currentProjectileData.Attack * damageBuffMul;
        addBuffProjectileData.ProjectileAddSpeed = currentProjectileData.ProjectileAddSpeed + accelerationBuffAdd;

        addBuffProjectileData.AttackType = currentProjectileData.AttackType == newProjectileAttackType
            ? currentProjectileData.AttackType
            : newProjectileAttackType;
        currentProjectileData.AttackType = addBuffProjectileData.AttackType;

        return addBuffProjectileData;
    }
}