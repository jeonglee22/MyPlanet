using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TowerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileOffset = 0.05f;
    private TowerTargetingSystem targetingSystem;
    public TowerTargetingSystem TargetingSystem => targetingSystem;
    private TowerDataSO towerData;
    public TowerDataSO AttackTowerData => towerData;
    private float shootTimer;

    private bool isOtherUserTower = false;
    public bool IsOtherUserTower { get { return isOtherUserTower; } set { isOtherUserTower = value; } }

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
    private float percentPenetrationBuffMul = 0f;
    public float PercentPenetrationBuffMul { get { return percentPenetrationBuffMul; } set { percentPenetrationBuffMul = value; } }
    private float fixedPenetrationBuffAdd = 0f;
    public float FixedPenetrationBuffAdd { get { return fixedPenetrationBuffAdd; } set { fixedPenetrationBuffAdd = value; } }
    private int targetNumberBuffAdd = 0;
    public int TargetNumberBuffAdd { get { return targetNumberBuffAdd; } set { targetNumberBuffAdd = value; } }
    private float hitRateBuffMul = 1f;

    public float BasicFireRate => towerData.fireRate;

    private float accuracyBuffAdd = 0f;
    public float AccuracyBuffAdd { get { return accuracyBuffAdd; } set { accuracyBuffAdd = value; } }
    public float HitRateBuffMultiplier => hitRateBuffMul;

    public float BasicHitRate => towerData.Accuracy;
    public float FinalHitRate
    {
        get
        {
            if (towerData == null) return 0f;

            float final = towerData.Accuracy * hitRateBuffMul;
            final += accuracyBuffAdd; 

            return final;
        }
    }

    //------------ Reinforce Field ---------------------
    [SerializeField] private int reinforceLevel = 0; //current level
    public int ReinforceLevel => reinforceLevel;
    [SerializeField] private float reinforceAttackScale = 1f; //balance
    private ProjectileData originalProjectileData; //original

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
    private float hitScanTimer;
    private bool isHitScanActive = false;
    public bool IsHitScanActive => isHitScanActive;

    public bool IsHaveHitScanAbility {get ; set;} = false;

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
    private bool isStartLazer = false;
    public bool IsStartLazer { get { return isStartLazer; } set { isStartLazer = value; } }
    private List<LazertowerAttack> lazers;
    public List<LazertowerAttack> Lazers => lazers;

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

        lazers = new List<LazertowerAttack>();
    }

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        if (towerData == null) return;
        //Connect Data Table To baseProjectileData(=currentProjectileData) 
        originalProjectileData = DataTableManager.ProjectileTable.Get(towerData.projectileIdFromTable);
        //Connect Tower Data
        towerData.projectileType = originalProjectileData;
        //apply reinforce
        if (originalProjectileData != null)
        {
            currentProjectileData = originalProjectileData.Clone();
        }
        //Set Projectile Count -> From Tower Data SO (NOT Data Table) -> check
        baseProjectileCount = Mathf.Max(1, towerData.projectileCount);

        if (towerData.towerIdInt == (int)AttackTowerId.Missile)
        {
            abilities.Add((int)AbilityId.Explosion);
        }
        //reinforce calculator
        RecalculateReinforcedBase();
    }

    private void Update()
    {
        if (towerData == null || targetingSystem == null) return;

        shootTimer += Time.deltaTime;

        //Fire Rate Buff Calculator
        float finalFireRate = CurrentFireRate;
        if (finalFireRate <= 0) return;
        float shootInterval = 1f / finalFireRate;

        float hitScanInterval = shootInterval * 0.5f;

        if(shootTimer >= hitScanInterval && !isHitScanActive && IsHaveHitScanAbility)
        {
            StartHitscan(hitScanInterval);
        }

        if(towerData.towerIdInt == (int)AttackTowerId.Lazer && isStartLazer)
        {
            return;
        }

        if(shootTimer>=shootInterval)
        {
            ShootAtTarget();
            shootTimer = 0f;
            hitScanTimer = 0f;
            isHitScanActive = false;
            targetingSystem.SetAttacking(false);
        }
    }

    private void StartHitscan(float hitScanInterval)
    {
        isHitScanActive = true;

        targetingSystem.SetAttacking(true);
        
        foreach (var target in targetingSystem.CurrentTargets)
        {
            if (target == null || !target.isAlive) continue;

            var obj = LoadManager.GetLoadedGamePrefab(ObjectName.HitScan);
            var hitScan = obj.GetComponent<HitScan>();
            hitScan.SetHitScan(target as Enemy, hitScanInterval);
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

        Vector3 baseDirection = (target.position - firePoint.position).normalized;
        float centerIndex = (shotCount - 1) * 0.5f;

        List<bool> innerIndex = new List<bool>(shotCount);
        if (towerData.towerIdInt == (int)AttackTowerId.ShootGun)
        {
            innerIndex = GetInnerShotCount(shotCount, towerData.grouping);
        }

        for (int i = 0; i < shotCount; i++)
        {
            float offsetIndex = i - centerIndex;
            var projectile = ProjectilePoolManager.Instance.GetProjectile(baseData);
            if (isOtherUserTower)
                projectile.IsOtherUser = true;

            var verticalDirection = new Vector3(-baseDirection.y, baseDirection.x, baseDirection.z).normalized;

            var direction = new Vector3(baseDirection.x, baseDirection.y, baseDirection.z);

            if (towerData.towerIdInt == (int)AttackTowerId.Lazer)
            {
                var lazerObj = LoadManager.GetLoadedGamePrefab(ObjectName.Lazer);
                var lazer = lazerObj.GetComponent<LazertowerAttack>();
                lazers.Add(lazer);

                projectile.Initialize(
                    buffedData,
                    baseData,
                    direction,
                    true,
                    ProjectilePoolManager.Instance.ProjectilePool
                );

                if (shotCount > 1)
                {
                    lazer.SetLazerPositionOffset(projectileOffset * (i - centerIndex));
                }
                lazer.SetLazer(transform, 0f, (target as Enemy).gameObject.transform, projectile, this, buffedData.RemainTime);
                isStartLazer = true;

                projectile.gameObject.transform.position = new Vector3(0, -1000f, 0);
                projectile.gameObject.SetActive(false);

                continue;
            }

            if(towerData.towerIdInt == (int)AttackTowerId.ShootGun && innerIndex != null && innerIndex.Count == shotCount)
            {
                ApplyGroupOffset(ref direction, innerIndex[i], towerData.grouping);
            }

            if (isHitScanActive && IsHaveHitScanAbility)
            {
                projectile.transform.position = target.position;
                projectile.transform.rotation = Quaternion.LookRotation(direction);
                Debug.Log(direction);

                SettingProjectile(projectile, buffedData, baseData, direction, attackType, target);
                continue;
            }

            ApplyAccuracyOffset(
                ref direction,
                towerData.Accuracy
            );

            projectile.transform.position =
                firePoint.position + verticalDirection * projectileOffset * offsetIndex;
            projectile.transform.rotation = Quaternion.LookRotation(direction);

            SettingProjectile(projectile, buffedData, baseData, direction, attackType, target);
        }
    }

    private List<bool> GetInnerShotCount(int shotCount, float grouping)
    {
        if (grouping <= 0f) return null;

        List<bool> innerIndex = new List<bool>();
        
        for (int i = 0; i < shotCount; i++)
        {
            innerIndex.Add(false);
        }

        int count = 0;
        while (count < shotCount * (grouping / 100f))
        {
            var index = UnityEngine.Random.Range(0, shotCount);

            if (innerIndex[index] == false)
            {
                innerIndex[index] = true;
                count++;
            }
        }

        return innerIndex;
    }

    private void SettingProjectile(Projectile projectile, ProjectileData buffedData, ProjectileData baseData, Vector3 direction, float attackType, ITargetable target)
    {
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

    private void ApplyGroupOffset(ref Vector3 direction, bool isInner, float grouping)
    {
        if (grouping <= 0f) return;

        var outerAngle = 45f;
        var innerAngle = 10f;

        float shootAngle = 0f;
        if (isInner)
        {
            shootAngle = UnityEngine.Random.Range(-innerAngle, innerAngle);
        }
        else
        {
            shootAngle = UnityEngine.Random.Range(0f, 1f) <= 0.5f ?
            UnityEngine.Random.Range(-outerAngle, -innerAngle) :
            UnityEngine.Random.Range(innerAngle, outerAngle);
        }

        Quaternion rot = Quaternion.Euler(0f, 0f, shootAngle);
        direction = rot * direction;
    }

    private void ApplyAccuracyOffset(ref Vector3 direction, float accuracy)
    {
        var rand01 = UnityEngine.Random.Range(0f, 1f);

        var finalAccuracy = accuracy * (1 + accuracyBuffAdd / 100f);

        if (rand01 < finalAccuracy / 100f)
        {
            return;
        }

        float offAngleMinus = UnityEngine.Random.Range(-1f,-0.5f) * 30f;
        float offAnglePlus = UnityEngine.Random.Range(0.5f,1f) * 30f;
        float[] angle = { offAngleMinus, offAnglePlus };
        float offAngle = angle[UnityEngine.Random.Range(0, 2)];
        
        Quaternion rot = Quaternion.Euler(0f, 0f, offAngle);
        direction = rot * direction;
    }

    public void AddAbility(int ability)
    {
        if (abilities == null) abilities = new List<int>();
        // if(!abilities.Contains(ability)) //preventing all duplication
            abilities.Add(ability);
    }
    public void RemoveAbility(int ability)
    {
        if (abilities == null) return;
        abilities.Remove(ability);
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
        addBuffProjectileData.RatePenetration = currentProjectileData.RatePenetration + (100f - currentProjectileData.RatePenetration) * percentPenetrationBuffMul;

        addBuffProjectileData.Attack = currentProjectileData.Attack * damageBuffMul;
        addBuffProjectileData.ProjectileAddSpeed = currentProjectileData.ProjectileAddSpeed + accelerationBuffAdd;

        addBuffProjectileData.AttackType = currentProjectileData.AttackType == newProjectileAttackType
            ? currentProjectileData.AttackType
            : newProjectileAttackType;
        currentProjectileData.AttackType = addBuffProjectileData.AttackType;

        return addBuffProjectileData;
    }

    //Reinforce ----------------------------------------------------
    public void SetReinforceLevel(int newLevel)
    {
        newLevel = Math.Max(0, newLevel);

        if (reinforceLevel == newLevel) return;
        reinforceLevel = newLevel;
        RecalculateReinforcedBase();
    }

    private void RecalculateReinforcedBase()
    {
        if (originalProjectileData == null || towerData == null)
            return;

        // copy to base
        currentProjectileData = originalProjectileData.Clone();

        // find AttackTowerTable Row 
        var attackRow = DataTableManager.AttackTowerTable.GetById(towerData.towerIdInt);
        if (attackRow == null) return;

        if (attackRow.TowerReinforceUpgrade_ID == null || attackRow.TowerReinforceUpgrade_ID.Length == 0) 
            return;

        float addValue = 0f;
        if (TowerReinforceManager.Instance != null)
        {
            addValue = TowerReinforceManager.Instance.GetAttackAddValueByIds(
                attackRow.TowerReinforceUpgrade_ID,
                reinforceLevel
            );
        }
        else
        {
            Debug.LogWarning("[AtkReinforce] TowerReinforceManager.Instance is null");
        }

        float finalAttack = (originalProjectileData.Attack + addValue) * reinforceAttackScale;
        currentProjectileData.Attack = finalAttack;
    }
}