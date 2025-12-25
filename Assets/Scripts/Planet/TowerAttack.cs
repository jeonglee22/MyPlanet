using System;
using System.Collections.Generic;
using System.Text;
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
    
    [Header("Debug")]
    [SerializeField] private bool debugBuffedProjectile = true;
    [SerializeField] private bool debugUpgradeEffects = true;

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

    //random ability
    //get from card
    private readonly List<int> ownedAbilityIds = new List<int>();
    private readonly Dictionary<int, IAbility> ownedAppliedInstances = new Dictionary<int, IAbility>();
    private readonly Dictionary<int, IAbility> appliedSelfAbilities = new Dictionary<int, IAbility>();
    //manage random ability
    private readonly Dictionary<TowerAmplifier,Dictionary<int, List<IAbility>>> ampAppliedInstances
        =new Dictionary<TowerAmplifier,Dictionary<int, List<IAbility>>>();

    [SerializeField] private bool debugReinforcedAbility = false;
    //------------------------------------------------
    private ProjectilePoolManager projectilePoolManager;

    //------------ Amplifier Buff Field ---------------------
    //damage
    private float damageBuffMul = 1f; //damage = baseDamage * damageBuffMul
    public float DamageBuffMul { get { return damageBuffMul; } set { damageBuffMul = value; } }
    private float accelerationBuffAdd = 0f;  // +=

    private readonly List<float> damageAbilitySources = new List<float>();
    private float damageAbilityMul = 1f; // self abilities only

    //fire rate --------------------------------------------------
    public float fireRateAbilityMul = 0f;
    private readonly List<float> fireRateAbilitySources = new List<float>();
    public float BasicFireRate => towerData.fireRate;
    public float fireRateBuffMul = 0f; //fireRate = baseFireRate + fireRateBuffMul
    private float towerUpgradeFireRateMul = 0f;
    public float CurrentFireRate
    {
        get
        {
            if (towerData == null) return 0f;
            float finalMul = 1f + fireRateAbilityMul + fireRateBuffMul + towerUpgradeFireRateMul;
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
    private int projectileCountFromUpgrade = 0;
    public int TotalProjectilecountvBuffAdd => projectileCountFromAmplifier + projectileCountFromAbility + projectileCountFromUpgrade;
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

    private float damageBuffFromUpgrade = 0f;

    private int additionalDurationFromUpgrade = 0;
    public int AdditionalDurationFromUpgrade => additionalDurationFromUpgrade;

    private float additionalExplosionRangeFromUpgrade = 0f;
    public float AdditionalExplosionRangeFromUpgrade => additionalExplosionRangeFromUpgrade;

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

    private Planet planet;

    private float totalDamageDealt = 0f;
    public float TotalDamageDealt => totalDamageDealt;

    //audio-------------------------------------------------
    [Header("Audio (Optional Override)")]
    [SerializeField] private AudioSource towerAudioSource;
    private bool laserLoopPlaying = false;
    //-------------------------------------------------------
    private void Awake()
    {
        targetingSystem = GetComponent<TowerTargetingSystem>();

        if (firePoint == null) firePoint = transform;

        projectilePoolManager = GameObject
            .FindGameObjectWithTag(TagName.ProjectilePoolManager)
            .GetComponent<ProjectilePoolManager>();

        lazers = new List<LazertowerAttack>();
        EnsureAudioSource();
    }

    private void Start()
    {
        planet = GameObject.FindWithTag(TagName.Planet).GetComponent<Planet>();
    }

    private void OnDisable()
    {
        DeleteExistLazers();
        StopLaserLoop();
    }

    public void AdddamageDealt(float damage)
    {
        totalDamageDealt += damage;
    }

    public void SetTowerData(TowerDataSO data)
    {
        towerData = data;
        if (towerData == null) return;

        StopLaserLoop();
        baseAbilityIds.Clear();
        amplifierAbilityIds.Clear();
        abilitiesDirty = true;

        var towerUpgradeData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        var towerIndex = towerUpgradeData.towerIds.IndexOf(towerData.towerIdInt);
        if (towerIndex != -1)
        {
            var upgradeLevel = towerUpgradeData.upgradeLevels[towerIndex];

            // Apply Upgrade Effects
            ApplyUpgradeEffects(upgradeLevel);
        }

        //damage
        damageAbilitySources.Clear();
        damageAbilityMul = 1f;

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

    private void ApplyUpgradeEffects(int upgradeLevel)
    {

        var additionalAttack = 0f;
        var additionalAttackSpeed = 0f;
        var additionalDuration = 0f;
        var additionalProjectileNum = 0;
        var additionalExplosionRange = 0f;

        var externalTowerUpgradeDataList = new List<TowerUpgradeData>();
        for (int i = 1; i <= upgradeLevel; i++)
        {
            var externalTowerUpgradeDataId = DataTableManager.TowerUpgradeTable.GetIdByTowerIdAndUpgradeCount(towerData.towerIdInt, i);
            if (externalTowerUpgradeDataId != -1)
            {
                var externalTowerUpgradeData = DataTableManager.TowerUpgradeTable.Get(externalTowerUpgradeDataId);
                externalTowerUpgradeDataList.Add(externalTowerUpgradeData);
            }
        }

        upgradeLevel = Mathf.Min(3, upgradeLevel);
        for (int i = 1; i <= upgradeLevel; i++)
        {
            var specialEffectId = externalTowerUpgradeDataList[i - 1].SpecialEffect_ID;
            var amount = externalTowerUpgradeDataList[i - 1].SpecialEffectValue;
            switch (specialEffectId)
            {
                case (int)SpecialEffectId.AttackSpeed:
                    additionalAttackSpeed += amount / 100f;
                    break;
                case (int)SpecialEffectId.Attack:
                    additionalAttack += amount / 100f;
                    break;
                case (int)SpecialEffectId.Duration:
                    additionalDuration += amount;
                    break;
                case (int)SpecialEffectId.ProjectileCount:
                    additionalProjectileNum += (int)amount;
                    break;
                case (int)SpecialEffectId.Explosion:
                    additionalExplosionRange += amount;
                    break;
                default:
                    break;   
            }
        }

        damageBuffFromUpgrade = additionalAttack;
        towerUpgradeFireRateMul = additionalAttackSpeed;
        additionalDurationFromUpgrade = (int)additionalDuration;
        projectileCountFromUpgrade = additionalProjectileNum;
        additionalExplosionRangeFromUpgrade = additionalExplosionRange;
        if (debugUpgradeEffects)
        {
            Debug.Log(
                $"[TowerAttack][ApplyUpgradeEffects] towerIdInt={(towerData != null ? towerData.towerIdInt : -1)} " +
                $"upgradeLevel={upgradeLevel}\n" +
                $"  -> damageBuffFromUpgrade(additionalAttack)={damageBuffFromUpgrade:0.###}\n" +
                $"  -> towerUpgradeFireRateMul(additionalAttackSpeed)={towerUpgradeFireRateMul:0.###}\n" +
                $"  -> additionalDurationFromUpgrade={additionalDurationFromUpgrade}\n" +
                $"  -> projectileCountFromUpgrade={projectileCountFromUpgrade}\n" +
                $"  -> additionalExplosionRangeFromUpgrade={additionalExplosionRangeFromUpgrade:0.###}"
            );
        }

    }

    private void Update()
    {
        if (towerData == null || targetingSystem == null) return;
        //audio
        if (towerData.towerIdInt == (int)AttackTowerId.Lazer)
        {
            if (isStartLazer)
            {
                StartLaserLoop();
                return; 
            }
            else
            {
                StopLaserLoop();
            }
        }

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
            DeleteExistLazers();

            ShootAtTarget();
            shootTimer = 0f;
            hitScanTimer = 0f;
            isHitScanActive = false;
            targetingSystem.SetAttacking(false);
        }
    }

    private void DeleteExistLazers()
    {
        foreach (var lazer in lazers)
        {
            if (lazer == null) continue;

            lazer?.gameObject.SetActive(false);
        }
        lazers.Clear();
        StopLaserLoop();
    }

    private void StartHitscan(float hitScanInterval)
    {
        if (towerData.towerIdInt == (int)AttackTowerId.Lazer)
            return;

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

            if (towerData.towerIdInt != (int)AttackTowerId.Lazer)
            {
                PlayShootOneShot();
            }

            var projectile = ProjectilePoolManager.Instance.GetProjectile(baseData);
            if (isOtherUserTower)
                projectile.IsOtherUser = true;

            projectile.damageEvent += AdddamageDealt;

            var verticalDirection = new Vector3(-baseDirection.y, baseDirection.x, baseDirection.z).normalized;

            var direction = new Vector3(baseDirection.x, baseDirection.y, baseDirection.z);

            if (towerData.towerIdInt == (int)AttackTowerId.Lazer)
            {
                StartLaserLoop();
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
                // Debug.Log(direction);

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
                    projectilePoolManager.ProjectilePool,
                    planet,
                    this.ReinforceLevel,
                    this.Abilities
        );

        if (attackType == (int)ProjectileType.Homing)
        {
            projectile.SetHomingTarget(target);
        }

        int shotCount = CurrentProjectileCount;
        if (shotCount > 1)
        {
            float projectileCountMul = GetAbilityDamageMultiplier(200011); 
            if (projectileCountMul < 1f) 
            {
                projectile.damageMultiplier *= projectileCountMul;
            }
        }

        int targetCount = targetingSystem != null ? targetingSystem.CurrentTargets.Count : 1;
        if (targetCount > 1)
        {
            float targetCountMul = GetAbilityDamageMultiplier(200012);
            if (targetCountMul < 1f)
            {
                projectile.damageMultiplier *= targetCountMul;
            }
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

            if (abilityId == (int)AbilityId.Explosion)
            {
                var explosionValue = DataTableManager.RandomAbilityTable.Get(abilityId).SpecialEffectValue;
                var totalExplosionRange = additionalExplosionRangeFromUpgrade + explosionValue;
                projectile.explosionRadius = totalExplosionRange;
            }
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
        ReapplyAllSelfAbilitiesByReinforce();
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

    //from card
    public void AddOwnedAbility(int abilityId)
    {
        if (abilityId <= 0) return;
        ownedAbilityIds.Add(abilityId);
        ApplyOwnedAbilityInstance(abilityId);
    }
    public void RemoveOwnedAbility(int abilityId)
    {
        if (abilityId <= 0) return;

        int idx = ownedAbilityIds.IndexOf(abilityId);
        if (idx >= 0) ownedAbilityIds.RemoveAt(idx);
        if(ownedAppliedInstances.TryGetValue(abilityId,out var inst)&&inst!=null)
        {
            inst.RemoveAbility(gameObject);
            ownedAppliedInstances.Remove(abilityId);
            if(debugReinforcedAbility)
                Debug.Log($"[OwnedAbility][REMOVE] tower={name}, abilityId={abilityId}, amount={inst.UpgradeAmount}");
        }
    }
    private void ApplyOwnedAbilityInstance(int abilityId)
    {
        if(ownedAppliedInstances.TryGetValue(abilityId,out var oldInst)&&oldInst!=null)
        {
            oldInst.RemoveAbility(gameObject);
            ownedAppliedInstances.Remove(abilityId);
        }
        var inst = ReinforceAbilityFactory.Create(abilityId, ReinforceLevel);
        if (inst == null) return;
        inst.ApplyAbility(gameObject);
        inst.Setting(gameObject);
        ownedAppliedInstances[abilityId] = inst;

        if (debugReinforcedAbility)
            Debug.Log($"[OwnedAbility][APPLY] tower={name}, abilityId={abilityId}, reinforce={ReinforceLevel}, amount={inst.UpgradeAmount}");
    }
    public void RebuildOwnedAbilityCache()
    {
        var unique = new HashSet<int>(ownedAbilityIds);
        foreach(var abilityId in unique)
        {
            ApplyOwnedAbilityInstance(abilityId);
        }
    }

    public void ApplyAmplifierAbilityReinforce(TowerAmplifier source, int abilityId, int sourceReinforceLevel)
    {
        if (source == null) return;
        if (abilityId <= 0) return;
        var inst = ReinforceAbilityFactory.Create(abilityId, sourceReinforceLevel);
        if (inst == null) return;
        inst.ApplyAbility(gameObject);
        inst.Setting(gameObject);
        if(!ampAppliedInstances.TryGetValue(source,out var byAbility))
        {
            byAbility = new Dictionary<int, List<IAbility>>();
            ampAppliedInstances[source] = byAbility;
        }
        if(!byAbility.TryGetValue(abilityId,out var list))
        {
            list = new List<IAbility>();
            byAbility[abilityId] = list;
        }
        list.Add(inst);
    }
    public void RemoveAmplifierAbilityReinforced(TowerAmplifier source, int abilityId, int count)
    {
        if (source == null) return;
        if (abilityId <= 0) return;
        if (count <= 0) return;

        if (!ampAppliedInstances.TryGetValue(source, out var byAbility)) return;
        if (!byAbility.TryGetValue(abilityId, out var list)) return;
        if (list == null || list.Count == 0) return;

        int removeCount = Mathf.Min(count, list.Count);

        // LIFO로 제거 (Apply 순서 역순)
        for (int i = 0; i < removeCount; i++)
        {
            int last = list.Count - 1;
            var inst = list[last];
            list.RemoveAt(last);

            if (inst != null)
                inst.RemoveAbility(gameObject);

            if (debugReinforcedAbility && inst != null)
                Debug.Log($"[AmpAbility][REMOVE] tower={name}, amp={source.name}, abilityId={abilityId}, amount={inst.UpgradeAmount}");
        }

        if (list.Count == 0)
            byAbility.Remove(abilityId);

        if (byAbility.Count == 0)
            ampAppliedInstances.Remove(source);
    }
    public void ClearAllAmplifierAbilitiesFrom(TowerAmplifier source)
    {
        if (source == null) return;

        if (!ampAppliedInstances.TryGetValue(source, out var byAbility)) return;

        foreach (var kv in byAbility)
        {
            var list = kv.Value;
            if (list == null) continue;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var inst = list[i];
                if (inst != null)
                    inst.RemoveAbility(gameObject);
            }
        }

        ampAppliedInstances.Remove(source);

        if (debugReinforcedAbility)
            Debug.Log($"[AmpAbility][CLEAR_SOURCE] tower={name}, amp={source.name}");
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
        // --- DEBUG: damage pipeline snapshot (BEFORE) ---
        if (debugBuffedProjectile)
        {
            float baseAtk = currentProjectileData != null ? currentProjectileData.Attack : -1f;
            float mulAmp = damageBuffMul;                 // amplifier (you said none -> should be 1)
            float mulAbility = damageAbilityMul;          // self ability sources
            float mulUpgrade = 1f + damageBuffFromUpgrade; // external upgrade (e.g. +0.05 => 1.05)

            float predicted = (currentProjectileData != null)
                ? baseAtk * mulAmp * mulAbility * mulUpgrade
                : -1f;

            Debug.Log(
                $"[TowerAttack][BuffedProj][BEFORE] towerIdInt={(towerData != null ? towerData.towerIdInt : -1)} " +
                $"reinforce={reinforceLevel} " +
                $"baseAtk={baseAtk:0.###} " +
                $"mulAmp(damageBuffMul)={mulAmp:0.###} " +
                $"mulAbility(damageAbilityMul)={mulAbility:0.###} " +
                $"mulUpgrade(1+damageBuffFromUpgrade)={mulUpgrade:0.###} (damageBuffFromUpgrade={damageBuffFromUpgrade:0.###}) " +
                $"predictedFinalAtk={predicted:0.###}"
            );
        }

        float rawAttack = currentProjectileData.Attack * damageBuffMul * damageAbilityMul * (1f + damageBuffFromUpgrade);
        // --- DEBUG: damage pipeline snapshot (AFTER) ---
        if (debugBuffedProjectile)
        {
            Debug.Log(
                $"[TowerAttack][BuffedProj][AFTER] towerIdInt={(towerData != null ? towerData.towerIdInt : -1)} " +
                $"buffedAtk={addBuffProjectileData.Attack:0.###} " +
                $"curProjAtk={currentProjectileData.Attack:0.###}"
            );
        }

        addBuffProjectileData.Attack = Mathf.Max(0f, rawAttack);
        addBuffProjectileData.ProjectileAddSpeed = currentProjectileData.ProjectileAddSpeed + accelerationBuffAdd;
        addBuffProjectileData.AttackType = currentProjectileData.AttackType == newProjectileAttackType
            ? currentProjectileData.AttackType
            : newProjectileAttackType;
        currentProjectileData.AttackType = addBuffProjectileData.AttackType;
        addBuffProjectileData.RemainTime =
            currentProjectileData.RemainTime + additionalDurationFromUpgrade;

        return addBuffProjectileData;
    }

    //Reinforce ----------------------------------------------------
    public void SetReinforceLevel(int newLevel)
    {
        newLevel = Math.Max(0, newLevel);

        if (reinforceLevel == newLevel) return;
        reinforceLevel = newLevel;

        ReapplyAllSelfAbilitiesByReinforce();
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
        Debug.Log($"[AddFireRate] ratePercent={ratePercent}, r={r}, before_count={fireRateAbilitySources.Count}");

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
        Debug.Log($"[RemoveFireRate] ratePercent={ratePercent}, r={r}, found_idx={idx}, before_count={fireRateAbilitySources.Count}");
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
    //Reinforce ------------------------------------------
    private IAbility CreateSelfAbilityInstanceWithReinforce(int abilityId)
    {
        return ReinforceAbilityFactory.Create(abilityId, ReinforceLevel);
    }
    private void ReapplyAllSelfAbilitiesByReinforce()
    {
        foreach (var kv in appliedSelfAbilities)
        {
            var ab = kv.Value;
            if (ab != null) ab.RemoveAbility(gameObject);
        }
        appliedSelfAbilities.Clear();

        foreach (var abilityId in baseAbilityIds)
        {
            var ab = CreateSelfAbilityInstanceWithReinforce(abilityId);
            if (ab == null) continue;
            ab.ApplyAbility(gameObject);
            ab.Setting(gameObject);
            appliedSelfAbilities[abilityId] = ab;
        }
        Debug.Log($"[Reapply] AFTER, fireRateAbilitySources.Count={fireRateAbilitySources.Count}");
    }

    //----------------------------------------------------
    //damage ---------------------------------------------
    public void AddDamageMulFromAbilitySource(float rate01)
    {
        float r = Mathf.Clamp(rate01, -0.99f, 10f); // rate01: 0.2f = +20%
        if (Mathf.Approximately(r, 0f)) return;

        damageAbilitySources.Add(r);
        RecalculateDamageMulFromAbility();
    }

    public void RemoveDamageMulFromAbilitySource(float rate01)
    {
        float r = Mathf.Clamp(rate01, -0.99f, 10f);

        int idx = damageAbilitySources.FindIndex(x => Mathf.Approximately(x, r));
        if (idx >= 0)
        {
            damageAbilitySources.RemoveAt(idx);
            RecalculateDamageMulFromAbility();
        }
    }

    private void RecalculateDamageMulFromAbility()
    {
        float sum = 0f;
        foreach (var r in damageAbilitySources)
            sum += r;
        damageAbilityMul = Mathf.Max(0f, 1f + sum);
    }
    //----------------------------------------------------
    public void ClearAllAmplifierAbilityStates()
    {
        amplifierAbilityIds.Clear();
        abilitiesDirty = true;

        foreach (var byAbility in ampAppliedInstances.Values)
        {
            foreach (var list in byAbility.Values)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    list[i]?.RemoveAbility(gameObject);
                }
            }
        }
        ampAppliedInstances.Clear();
    }
    //audio ---------------------------------------
    private void EnsureAudioSource()
    {
        if (towerAudioSource != null) return;
        if (firePoint != null)
        {
            towerAudioSource = firePoint.GetComponent<AudioSource>();
        }

        if (towerAudioSource == null)
        {
            towerAudioSource = GetComponent<AudioSource>();
        }

        if (towerAudioSource == null)
        {
            towerAudioSource = gameObject.AddComponent<AudioSource>();
        }
        towerAudioSource.playOnAwake = false;
        towerAudioSource.loop = false;
    }

    private void PlayShootOneShot()
    {
        if (towerData == null) return;
        if (towerData.shootSfx == null) return;

        EnsureAudioSource();

        float originalPitch = towerAudioSource.pitch;
        Vector2 pr = towerData.shootPitchRange;

        float p = (Mathf.Approximately(pr.x, pr.y))
            ? pr.x
            : UnityEngine.Random.Range(Mathf.Min(pr.x, pr.y), Mathf.Max(pr.x, pr.y));

        towerAudioSource.pitch = p;
        towerAudioSource.PlayOneShot(towerData.shootSfx, towerData.shootVolume);
        towerAudioSource.pitch = originalPitch;
    }

    private void StartLaserLoop()
    {
        if (towerData == null) return;
        if (towerData.laserLoopSfx == null) return;

        EnsureAudioSource();

        if (laserLoopPlaying && towerAudioSource.isPlaying && towerAudioSource.clip == towerData.laserLoopSfx && towerAudioSource.loop)
            return;

        towerAudioSource.Stop();
        towerAudioSource.clip = towerData.laserLoopSfx;
        towerAudioSource.loop = true;
        towerAudioSource.volume = towerData.laserLoopVolume;
        towerAudioSource.Play();

        laserLoopPlaying = true;
    }

    private void StopLaserLoop()
    {
        if (towerAudioSource == null) { laserLoopPlaying = false; return; }

        if (towerAudioSource.loop)
        {
            towerAudioSource.loop = false;
            towerAudioSource.Stop();
            towerAudioSource.clip = null;
        }

        laserLoopPlaying = false;
    }
    //---------------------------------------------

    private float GetAbilityDamageMultiplier(int abilityId)
    {
        if (!Abilities.Contains(abilityId))
            return 1f;

        if (!DataTableManager.IsInitialized)
            return 1f;

        var ra = DataTableManager.RandomAbilityTable?.Get(abilityId);
        if (ra == null)
            return 1f;

        if (ra.RandomAbilityType == 1)
        {
            if (TowerReinforceManager.Instance == null)
                return 1f;

            return TowerReinforceManager.Instance
                .GetFinalSuperValueForAbility(abilityId, ReinforceLevel);
        }
        return 1f;
    }
}