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

    private readonly List<AmplifierTowerDataSO> activeAmplifierBuffs
        = new List<AmplifierTowerDataSO>();

    private float shootTimer;

    private bool isOtherUserTower = false;
    public bool IsOtherUserTower { get { return isOtherUserTower; } set { isOtherUserTower = value; } }

    private List<IAbility> testAbilities;
    public List<IAbility> TestAbilities { get { return testAbilities; } set { testAbilities = value; } }

    //test
    //ability ----------------------------------------
    //------------------------------------------------
    //ability from attack
    private readonly List<int> baseAbilityIds = new List<int>();
    //ability from buff
    private readonly Dictionary<TowerAmplifier, List<int>> amplifierAbilityIds = new Dictionary<TowerAmplifier, List<int>>();
    //merged ability list
    private readonly List<int> mergedAbilityIds = new List<int>();
    private bool abilitiesDirty = true;
    public List<int> Abilities
    {
        get
        {
            if (abilitiesDirty) RebuildMergedAbilities();
            return mergedAbilityIds;
        }
    }
    //------------------------------------------------
    private ProjectilePoolManager projectilePoolManager;

    //------------ Amplifier Buff Field ---------------------
    private float damageBuffMul = 1f; //damage = baseDamage * damageBuffMul
    public float DamageBuffMul { get { return damageBuffMul; } set { damageBuffMul = value; } }
    private float accelerationBuffAdd = 0f;  // +=

    //fire rate --------------------------------------------------
    public float fireRateAbilityMul = 0f;
    private readonly List<float> fireRateAbilitySources = new List<float>();
    public float BasicFireRate => towerData.fireRate;
    public float fireRateBuffMul = 0f; //fireRate = baseFireRate + fireRateBuffMul
    public float CurrentFireRate
    {
        get
        {
            if (towerData == null) return 0f;
            float finalMul = 1f + fireRateAbilityMul + fireRateBuffMul;
            return towerData.fireRate * finalMul;
        }
    }
    //------------------------------------------------------------
    private int newProjectileAttackType;
    public int NewProjectileAttackType { get { return newProjectileAttackType; } set { newProjectileAttackType = value; } }
    //hit radius ----------------------
    private float ampHitRadiusMul = 1f;
    private float hitRadiusBuffMul = 1f;
    public float HitRadiusBuffMul => hitRadiusBuffMul;
    private readonly System.Collections.Generic.List<float> hitRadiusAbilitySources
        = new System.Collections.Generic.List<float>();
    //---------------------------------

    //percentpenetration ---------------
    private float percentPenetrationFromAbility = 0f;
    public float PercentPenetrationBuffMul
    {
        get => percentPenetrationFromAbility;
        set => percentPenetrationFromAbility = Mathf.Clamp01(value);
    }
    private float percentPenetrationFromAmplifier = 0f;

    private readonly System.Collections.Generic.List<float> percentPenAbilitySources
        = new System.Collections.Generic.List<float>();
    //fixed-------------------------------
    private float fixedPenetrationBuffAdd = 0f;
    public float FixedPenetrationBuffAdd { get { return fixedPenetrationBuffAdd; } set { fixedPenetrationBuffAdd = value; } }
    private float fixedPenetrationFromAmplifier = 0f;
    //TargetNum---------------------------
    private int targetNumberFromAbility = 0;
    private int targetNumberFromAmplifier = 0;
    public int TargetNumberFromAmplifier => targetNumberFromAmplifier;
    public int TargetNumberBuffAdd
    {
        get => targetNumberFromAbility;
        set => targetNumberFromAbility = value;
    }
    private int TotalTargetNumberBuffAdd => targetNumberFromAbility + targetNumberFromAmplifier;
    public int TotalTargetNumberExtra => TotalTargetNumberBuffAdd;
    private int lastAppliedAmplifierTargetExtra = 0;
    //------------------------------------

    private float accuracyBuffAdd = 0f;
    private float accuracyFromAmplifier = 0f;
    public float AccuracyBuffAdd { get { return accuracyBuffAdd; } set { accuracyBuffAdd = value; } }
    private float hitRateBuffMul = 1f;
    public float HitRateBuffMultiplier => hitRateBuffMul;

    public float BasicHitRate => towerData.Accuracy;
    public float FinalHitRate
    {
        get
        {
            if (towerData == null) return 0f;

            float baseAcc = towerData.Accuracy;
            float final = baseAcc
                          + accuracyFromAmplifier
                          + accuracyBuffAdd;
            return Mathf.Clamp(final, 0f, 100f);
        }
    }

    //------------ Reinforce Field ---------------------
    [SerializeField] private int reinforceLevel = 0; //current level
    public int ReinforceLevel => reinforceLevel;
    [SerializeField] private float reinforceAttackScale = 1f; //balance
    private ProjectileData originalProjectileData; //original

    //projectile Count---------------------------------------
    private int baseProjectileCount = 1; //Only Var
    private int projectileCountFromAmplifier = 0;
    private int projectileCountFromAbility = 0;
    public int ProjectileCountFromAbility
    {
        get => projectileCountFromAbility;
        set => projectileCountFromAbility = value;
    }
    public int ProjectileCountFromAmplifier => projectileCountFromAmplifier;
    public int TotalProjectilecountvBuffAdd => projectileCountFromAmplifier + projectileCountFromAbility;
    public int CurrentProjectileCount
    {
        get
        {
            int finalCount = baseProjectileCount + TotalProjectilecountvBuffAdd;
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

    public bool IsHaveHitScanAbility { get; set; } = false;

    public ProjectileData BaseProjectileData => currentProjectileData;
    public ProjectileData BuffedProjectileData => CurrentProjectileData;
    public ProjectileData CurrentProjectileData
    {
        get
        {
            if (asyncUserProjectileData != null)
            {
                return asyncUserProjectileData;
            }

            var buffed = GetBuffedProjectileData();
            if (buffed != null) return buffed;
            return towerData != null ? towerData.projectileType : null;
        }
        set
        {
            asyncUserProjectileData = value;
        }
    }
    private ProjectileData asyncUserProjectileData;

    private bool isStartLazer = false;
    public bool IsStartLazer { get { return isStartLazer; } set { isStartLazer = value; } }
    private List<LazertowerAttack> lazers;
    public List<LazertowerAttack> Lazers => lazers;

    //-------------------------------------------------------
    private void Awake()
    {
        targetingSystem = GetComponent<TowerTargetingSystem>();

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

        baseAbilityIds.Clear();
        amplifierAbilityIds.Clear();
        abilitiesDirty = true;

        //firerate
        fireRateAbilitySources.Clear();
        fireRateAbilityMul = 0f;
        fireRateBuffMul = 0f;

        //target count
        targetNumberFromAbility = 0;
        targetNumberFromAmplifier = 0;
        lastAppliedAmplifierTargetExtra = 0;

        //fixedPenetration
        fixedPenetrationBuffAdd = 0f;
        fixedPenetrationFromAmplifier = 0f;

        //Connect Data Table To baseProjectileData(=currentProjectileData) 
        originalProjectileData = DataTableManager.ProjectileTable.Get(towerData.projectileIdFromTable);
        //Connect Tower Data
        towerData.projectileType = originalProjectileData;
        //apply reinforce
        if (originalProjectileData != null)
        {
            currentProjectileData = originalProjectileData.Clone();
        }
        //prefab visual mapping
        if(projectilePoolManager!=null&&originalProjectileData!=null&&towerData.projectilePrefab!=null)
        {
            projectilePoolManager.RegisterProjectilePrefab(
                originalProjectileData,towerData.projectilePrefab);
        }
        //Set Projectile Count -> From Tower Data SO (NOT Data Table) -> check
        baseProjectileCount = Mathf.Max(1, towerData.projectileCount);

        if (towerData.towerIdInt == (int)AttackTowerId.Missile)
        {
            AddBaseAbility((int)AbilityId.Explosion);
        }
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

        if (shootTimer >= hitScanInterval && !isHitScanActive && IsHaveHitScanAbility)
        {
            StartHitscan(hitScanInterval);
        }

        if (towerData.towerIdInt == (int)AttackTowerId.Lazer && isStartLazer)
        {
            return;
        }

        if (shootTimer >= shootInterval)
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

        List<ITargetable> validTargets = new List<ITargetable>();
        if (targets != null)
        {
            foreach (var t in targets)
            {
                if (t != null && t.isAlive) validTargets.Add(t);
            }
        }

        //debug
        int targetCount =
       (targets != null && targets.Count > 0)
       ? targets.Count
       : (targetingSystem.CurrentTarget != null ? 1 : 0);

        int expectedProjectiles = targetCount * shotCount;

        // Debug.Log(
        //     $"[ShootAtTarget] {gameObject.name} " +
        //     $"targetCount={targetCount}, shotCount={shotCount}, " +
        //     $"expectedProjectiles={expectedProjectiles}, " +
        //     $"baseProjCount={baseProjectileCount}, " +
        //     $"extraProjFromAbility={projectileCountFromAbility}, " +
        //     $"extraProjFromAmp={projectileCountFromAmplifier}, " +
        //     $"baseTargets={ (targetingSystem != null ? targetingSystem.BaseTargetCount : 0) }, " +
        //     $"extraTargets={TotalTargetNumberBuffAdd}"
        // );
        //

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

//         Debug.Log(
//     $"[FireToTarget] {gameObject.name} " +
//     $"target={target} shotCount={shotCount}"
// );

        Vector3 baseDirection = (target.position - firePoint.position).normalized;
        float centerIndex = (shotCount - 1) * 0.5f;

        List<bool> innerIndex = new List<bool>(shotCount);
        if (towerData.towerIdInt == (int)AttackTowerId.ShootGun)
        {
            innerIndex = GetInnerShotCount(shotCount, towerData.grouping);
        }

        for (int i = 0; i < shotCount; i++)
        {
    //         Debug.Log(
    //        $"[FireToTargetLoop] {gameObject.name} " +
    //        $"target={target} projIndex={i + 1}/{shotCount}"
    //    );

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

                float baseSize = baseData != null ? baseData.CollisionSize : buffedData.CollisionSize;
                float finalSize = buffedData.CollisionSize;
                float hitSizeFactor = 1f;
                if (baseSize > 0f)
                    hitSizeFactor = finalSize / baseSize;

                if (shotCount > 1)
                {
                    lazer.SetLazerPositionOffset(projectileOffset * (i - centerIndex));
                }
                lazer.SetLazer(
                    transform,
                    0f,
                    (target as Enemy).gameObject.transform,
                    projectile,
                    this,
                    buffedData.RemainTime
                );
                isStartLazer = true;

                projectile.gameObject.transform.position = new Vector3(0, -1000f, 0);
                projectile.gameObject.SetActive(false);

                continue;
            }

            if (towerData.towerIdInt == (int)AttackTowerId.ShootGun && innerIndex != null && innerIndex.Count == shotCount)
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

        if (Variables.IsTestMode)
        {
            foreach (var ability in testAbilities)
            {
                ability.Setting(gameObject);
                ability.ApplyAbility(projectile.gameObject);
                projectile.abilityAction += ability.ApplyAbility;
                projectile.abilityRelease += ability.RemoveAbility;
            }
            return;
        }

        foreach (var abilityId in Abilities)
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

        float finalAccuracy = Mathf.Clamp(FinalHitRate, 0f, 100f);

        if (rand01 < finalAccuracy / 100f)
        {
            return;
        }

        float offAngleMinus = UnityEngine.Random.Range(-1f, -0.5f) * 30f;
        float offAnglePlus = UnityEngine.Random.Range(0.5f, 1f) * 30f;
        float[] angle = { offAngleMinus, offAnglePlus };
        float offAngle = angle[UnityEngine.Random.Range(0, 2)];

        Quaternion rot = Quaternion.Euler(0f, 0f, offAngle);
        direction = rot * direction;
    }

    //ability ------------------------------------------------
    private void RebuildMergedAbilities()
    {
        mergedAbilityIds.Clear();
        mergedAbilityIds.AddRange(baseAbilityIds);
        foreach (var kv in amplifierAbilityIds)
        {
            var list = kv.Value;
            if (list == null) continue;
            mergedAbilityIds.AddRange(list);
        }
        abilitiesDirty = false;
    }
    public void AddBaseAbility(int abilityId)
    {
        if (!baseAbilityIds.Contains(abilityId))
        {
            baseAbilityIds.Add(abilityId);
            abilitiesDirty = true;
        }
    }

    public void AddAmplifierAbility(TowerAmplifier source, int abilityId)
    {
        if (source == null) return;
        if (!amplifierAbilityIds.TryGetValue(source, out var list))
        {
            list = new List<int>();
            amplifierAbilityIds[source] = list;
        }
        list.Add(abilityId);
        abilitiesDirty = true;
    }

    public void RemoveAmplifierAbility(TowerAmplifier source, int abilityId, int count = 1)
    {
        if (source == null) return;
        if (!amplifierAbilityIds.TryGetValue(source, out var list)) return;

        for (int i = 0; i < count; i++)
        {
            int idx = list.IndexOf(abilityId);
            if (idx < 0) break;
            list.RemoveAt(idx);
        }

        if (list.Count == 0)
            amplifierAbilityIds.Remove(source);

        abilitiesDirty = true;
    }

    public void ClearAmplifierAbilitiesFromSource(TowerAmplifier source)
    {
        if (source == null) return;
        if (amplifierAbilityIds.Remove(source))
        {
            abilitiesDirty = true;
        }
    }

    public void AddAbility(int ability)
    {
        AddBaseAbility(ability);
    }
    public void RemoveAbility(int ability)
    {
        int idx = baseAbilityIds.IndexOf(ability);
        if (idx >= 0)
        {
            baseAbilityIds.RemoveAt(idx);
            abilitiesDirty = true;
        }
    }

    public void SetRandomAbility()
    {
        var ability = AbilityManager.GetRandomAbility();
        AddBaseAbility(ability);
        AbilityManager.GetAbility(ability)?.ApplyAbility(gameObject);
        AbilityManager.GetAbility(ability)?.Setting(gameObject);
    }

    //--------------------------------------------------------
    public void SetUpBuff(AmplifierTowerDataSO amp)
    {
        if (amp == null)
        {
            ClearAllAmplifierBuffs();
        }
        else
        {
            AddAmplifierBuff(amp);
        }
    }

    public void AddAmplifierBuff(AmplifierTowerDataSO amp)
    {
        if (amp == null) return;

        activeAmplifierBuffs.Add(amp);
        RecalculateAmplifierBuffs();
    }

    public void RemoveAmplifierBuff(AmplifierTowerDataSO amp)
    {
        if (amp == null) return;

        int idx = activeAmplifierBuffs.FindIndex(a => a == amp);
        if (idx >= 0)
        {
            activeAmplifierBuffs.RemoveAt(idx);
            RecalculateAmplifierBuffs();
        }
    }

    public void ClearAllAmplifierBuffs()
    {
        if (activeAmplifierBuffs.Count == 0) return;

        activeAmplifierBuffs.Clear();
        RecalculateAmplifierBuffs();
    }

    public void RecalculateAmplifierBuffs()
    {
        damageBuffMul = 1f;
        fireRateBuffMul = 0f;
        accelerationBuffAdd = 0f;
        projectileCountFromAmplifier = 0;

        ampHitRadiusMul = 1f;
        percentPenetrationFromAmplifier = 0f;
        targetNumberFromAmplifier = 0;
        accuracyFromAmplifier = 0f;
        float oldAmpFixed = fixedPenetrationFromAmplifier;
        fixedPenetrationFromAmplifier = 0f;

        foreach (var amp in activeAmplifierBuffs)
        {
            if (amp == null) continue;

            damageBuffMul += amp.DamageBuff;

            float fireRateMul = Mathf.Max(0f, amp.FireRateBuff);
            fireRateBuffMul += (fireRateMul - 1f);

            accelerationBuffAdd += amp.AccelerationBuff;
            projectileCountFromAmplifier += amp.ProjectileCountBuff;

            ampHitRadiusMul *= (1f + amp.HitRadiusBuff / 100f);

            percentPenetrationFromAmplifier =
                CombinePercentPenetration01(percentPenetrationFromAmplifier, amp.PercentPenetrationBuff);

            fixedPenetrationFromAmplifier += amp.FixedPenetrationBuff;

            // Debug.Log(
            //     $"[AmpBase][FixedPen] tower={name}, amp={amp.name}, " +
            //     $"amp.FixedPenetrationBuff={amp.FixedPenetrationBuff}, " +
            //     $"sumAmpFixed={fixedPenetrationFromAmplifier}"
            // );
            targetNumberFromAmplifier += amp.TargetNumberBuff;
            accuracyFromAmplifier += amp.HitRateBuff;
        }
        damageBuffMul = Mathf.Max(0f, damageBuffMul);

        RecalculateHitRadiusBuffMul();

        if (targetingSystem != null)
        {
            int amplifierExtra = targetNumberFromAmplifier;
            int delta = amplifierExtra - lastAppliedAmplifierTargetExtra;
            if (delta != 0) targetingSystem.AddExtraTargetCount(delta);
            lastAppliedAmplifierTargetExtra = amplifierExtra;
        }
    }

    private ProjectileData GetBuffedProjectileData() // making runtime once
    {
        if (currentProjectileData == null) return null;

        addBuffProjectileData = currentProjectileData.Clone();

        float basePierce = currentProjectileData.TargetNum;
        addBuffProjectileData.TargetNum = Mathf.Max(1, basePierce);

        //hit radius------
        float finalHitRadiusMul = hitRadiusBuffMul;
        addBuffProjectileData.CollisionSize =
            currentProjectileData.CollisionSize * finalHitRadiusMul;
        //----------------
        //fixed-----------
        float baseFixed = currentProjectileData.FixedPenetration;
        float fromAbility = fixedPenetrationBuffAdd;
        float fromAmpBase = fixedPenetrationFromAmplifier;
        addBuffProjectileData.FixedPenetration =
       baseFixed + fromAbility + fromAmpBase;

        // Debug.Log(
        //     $"[FixedPen][Calc] tower={name} base={baseFixed}, fromAbility={fromAbility}, " +
        //     $"fromAmpBase={fromAmpBase}"
        // );
        //------------------
        //rate penetration---------------------------
        float baseRate01 = Mathf.Clamp01(currentProjectileData.RatePenetration / 100f);
        float ability01 = Mathf.Clamp01(percentPenetrationFromAbility);
        float amp01 = Mathf.Clamp01(percentPenetrationFromAmplifier);
        float oneMinus = (1f - baseRate01)* (1f - ability01)* (1f - amp01);
        float finalRate01 = 1f - oneMinus;
        addBuffProjectileData.RatePenetration =
            Mathf.Clamp(finalRate01 * 100f, 0f, 100f);
        //-------------------------------------------
        float rawAttack = currentProjectileData.Attack * damageBuffMul;

        addBuffProjectileData.Attack = Mathf.Max(0f, rawAttack);
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
        finalAttack = Mathf.Max(0f, finalAttack);

        currentProjectileData.Attack = finalAttack;
    }

    //penetration ----------------------------------
    private float CombinePercentPenetration01(float current, float add)
    {
        current = Mathf.Clamp01(current);
        add = Mathf.Clamp01(add);

        return 1f - (1f - current) * (1f - add);
    }

    public void AddPercentPenetrationFromAbilitySource(float add)
    {
        add = Mathf.Clamp01(add);
        if (add <= 0f) return;

        percentPenAbilitySources.Add(add);
        RecalculatePercentPenetrationFromAbility();
    }

    public void RemovePercentPenetrationFromAbilitySource(float add)
    {
        add = Mathf.Clamp01(add);
        if (add <= 0f) return;

        int idx = percentPenAbilitySources.FindIndex(p => Mathf.Approximately(p, add));
        if (idx >= 0)
            percentPenAbilitySources.RemoveAt(idx);

        RecalculatePercentPenetrationFromAbility();
    }

    private void RecalculatePercentPenetrationFromAbility()
    {
        if (percentPenAbilitySources.Count == 0)
        {
            percentPenetrationFromAbility = 0f;
            return;
        }

        float oneMinusMul = 1f;
        foreach (float p in percentPenAbilitySources)
        {
            oneMinusMul *= (1f - Mathf.Clamp01(p));
        }

        percentPenetrationFromAbility = 1f - oneMinusMul;
    }
    //----------------------------------------------
    //hit radius ------------------------------
    public void AddHitRadiusFromAbilitySource(float rate)
    {
        float r = Mathf.Clamp(rate, -0.99f, 10f);
        if (Mathf.Approximately(r, 0f)) return;

        hitRadiusAbilitySources.Add(r);
        RecalculateHitRadiusBuffMul();
    }

    public void RemoveHitRadiusFromAbilitySource(float rate)
    {
        float r = Mathf.Clamp(rate, -0.99f, 10f);
        int idx = hitRadiusAbilitySources.FindIndex(x => Mathf.Approximately(x, r));
        if (idx >= 0)
        {
            hitRadiusAbilitySources.RemoveAt(idx);
            RecalculateHitRadiusBuffMul();
        }
    }

    private void RecalculateHitRadiusBuffMul()
    {
        float abilityMul = 1f;

        foreach (var r in hitRadiusAbilitySources)
        {
            abilityMul *= (1f + r);
        }
        hitRadiusBuffMul = ampHitRadiusMul * abilityMul;
    }
    //-----------------------------------------
    //fireRate----------------------------------
    public void AddFireRateFromAbilitySource(float ratePercent)
    {
        float r = Mathf.Clamp(ratePercent / 100f, -0.99f, 10f);
        if (Mathf.Approximately(r, 0f)) return;

        fireRateAbilitySources.Add(r);
        RecalculateFireRateFromAbility();
    }

    public void RemoveFireRateFromAbilitySource(float ratePercent)
    {
        float r = Mathf.Clamp(ratePercent / 100f, -0.99f, 10f);

        int idx = fireRateAbilitySources.FindIndex(x => Mathf.Approximately(x, r));
        if (idx >= 0)
        {
            fireRateAbilitySources.RemoveAt(idx);
            RecalculateFireRateFromAbility();
        }
    }

    private void RecalculateFireRateFromAbility()
    {
        float sum = 0f;
        foreach (var r in fireRateAbilitySources)
        {
            sum += r;
        }
        fireRateAbilityMul = sum;
    }
    //----------------------------------------------------
}