using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class TowerAttackTester : MonoBehaviour
{
    public int towerAttackId = -1;
    private TowerAttack towerAttack;
    public float damage = 0f;
    public float attackSpeed = 0f;
    public int targetRange = -1;
    [Range(0f, 100f)]
    public float accuracy = 50f;
    [Range(0f, 100f)]
    public float grouping = 0f;
    public int projectileNum = 1;
    public int targetNum = 1;
    public float hitSize = 10f;
    [Range(0f, 100f)]
    public float ratePenetration = 0f;
    public float fixedPenetration = 0;
    public float projectileSpeed = 1f;
    public float duration = 10f;

    private int currentTowerAttackId = -1;
    private TowerDataSO choosedTowerData;
    [SerializeField] private TowerDataSO[] towerDataSOs;
    [SerializeField] private TargetRangeSO[] targetRangeSOs;
    private ProjectileData projectileData;

    public float explosionRadius;
    private float currentExplosionRadius;
    public int homing;
    private int currentHoming;
    public int splitCount;
    private int currentSplitCount;
    public float slowPercent;
    private float currentSlowPercent;
    public int chainCount;
    private int currentChainCount;
    public int pierceCount;
    private int currentPierceCount;

    private List<IAbility> abilities;

    private async UniTaskVoid Start()
    {
        Initialize();

        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        abilities = new List<IAbility>();
    }

    private void Initialize()
    {
        towerAttackId = -1;
        targetRange = -1;
        damage = 0f;
        attackSpeed = 0f;
        accuracy = 50f;
        grouping = 0f;
        projectileNum = 0;
        targetNum = 1;
        hitSize = 10f;
        ratePenetration = 0f;
        fixedPenetration = 0;
        projectileSpeed = 1f;
        duration = 10f;
        currentTowerAttackId = -1;

        explosionRadius = 0f;
        homing = 0;
        splitCount = 0;
        slowPercent = 0f;
        chainCount = 0;
        pierceCount = 0;

        currentChainCount = 0;
        currentExplosionRadius = 0f;
        currentHoming = 0;
        currentPierceCount = 0;
        currentSplitCount = 0;
        currentSlowPercent = 0f;
    }

    void FixedUpdate()
    {
        TestBalance();
        UpdateProjectileData();
        UpdateAbility();
    }

    private void UpdateAbility()
    {
        if (projectileData == null)
            return;

        if (towerAttack.Abilities == null)
            return;

        abilities.Clear();

        if (explosionRadius > 0f)
        {
            var explosionAbility = new ExplosionAbility(explosionRadius);
            abilities.Add(explosionAbility);
        }

        if (homing > 0)
        {
            var homingAbility = new HomingUpgradeAbility(homing);
            abilities.Add(homingAbility);
            homingAbility.ApplyAbility(towerAttack.gameObject);
        }
        else
        {
            var homingAbility = new HomingUpgradeAbility(0);
            homingAbility.RemoveAbility(towerAttack.gameObject);
        }

        if (splitCount > 0)
        {
            var splitAbility = new SplitUpgradeAbility(splitCount);
            abilities.Add(splitAbility);
            splitAbility.Setting(towerAttack.gameObject);
        }

        if (slowPercent > 0f)
        {
            var paralyzeAbility = new ParalyzeAbility(slowPercent);
            abilities.Add(paralyzeAbility);
        }

        if (chainCount > 0)
        {
            var chainAbility = new ChainUpgradeAbility(chainCount);
            abilities.Add(chainAbility);
        }
        
        if (pierceCount > 0)
        {
            var pierceAbility = new PierceUpgradeAbility(pierceCount);
            abilities.Add(pierceAbility);
        }
        
        towerAttack.TestAbilities = abilities;

        // if (towerAttack.Abilities.Count == 0)
        // {
        //     towerAttack.AddAbility((int)AbilityId.Homing);
        //     towerAttack.AddAbility((int)AbilityId.Explosion);
        //     towerAttack.AddAbility((int)AbilityId.Split);
        //     towerAttack.AddAbility((int)AbilityId.Slow);
        //     towerAttack.AddAbility((int)AbilityId.Chain);
        //     towerAttack.AddAbility((int)AbilityId.Pierce);
        //     towerAttack.TestAbilities = abilities;
        // }

        // for (int i = 0; i < abilities.Count; i++)
        // {
        //     switch (abilities[i])
        //     {
        //         case HomingUpgradeAbility:
        //             if (currentHoming == homing)
        //                 continue;

        //             currentHoming = homing;
        //             Debug.Log("Homing" + currentHoming);
        //             if (currentHoming == 1)
        //                 abilities[i].ApplyAbility(towerAttack.gameObject);
        //             else
        //                 abilities[i].RemoveAbility(towerAttack.gameObject);
        //             break;
        //         case ExplosionAbility:
        //             if (Mathf.Approximately(currentExplosionRadius, explosionRadius))
        //                 continue;

        //             currentExplosionRadius = explosionRadius;
                    
        //             Debug.Log("Explosion: " + currentExplosionRadius);
        //             var newExplosionAbility = new ExplosionAbility(currentExplosionRadius);
        //             abilities[i] = newExplosionAbility;
        //             break;
        //         case SplitUpgradeAbility:
        //             if (currentSplitCount == splitCount)
        //                 continue;

        //             currentSplitCount = splitCount;

        //             Debug.Log("Split:" + currentSplitCount);
        //             var newSplitUpgradeAbility = new SplitUpgradeAbility(currentSplitCount);
        //             abilities[i] = newSplitUpgradeAbility;
        //             break;
        //         case ParalyzeAbility:
        //             if (Mathf.Approximately(currentSlowPercent, slowPercent))
        //                 continue;

        //             currentSlowPercent = slowPercent;

        //             Debug.Log("SlowAbility:" + currentSlowPercent);
        //             var newParalyzeAbility = new ParalyzeAbility(currentSlowPercent);
        //             abilities[i] = newParalyzeAbility;
        //             break;
        //         case ChainUpgradeAbility:
        //             if (currentChainCount == chainCount)
        //                 continue;

        //             currentChainCount = chainCount;

        //             Debug.Log("ChainAttack:" + currentChainCount);
        //             var newChainUpgradeAbility = new ChainUpgradeAbility(currentChainCount);
        //             abilities[i] = newChainUpgradeAbility;
        //             break;
        //         case PierceUpgradeAbility:
        //             if (currentPierceCount == pierceCount)
        //                 continue;

        //             currentPierceCount = pierceCount;

        //             Debug.Log("Pierce" + currentPierceCount);
        //             var newPierceUpgradeAbility = new PierceUpgradeAbility(currentPierceCount);
        //             abilities[i] = newPierceUpgradeAbility;
        //             break;
        //     }
        // }
    }

    private void UpdateProjectileData()
    {
        if (projectileData == null || towerAttack == null)
            return;

        projectileData.Attack = damage;
        towerAttack.AttackTowerData.fireRate = attackSpeed;
        towerAttack.AttackTowerData.Accuracy = accuracy;
        towerAttack.AttackTowerData.grouping = grouping;
        towerAttack.BaseProjectileCount = (int)projectileNum;
        towerAttack.TargetingSystem.SetExtraTargetCount(targetNum - 1 < 0 ? 0 : targetNum - 1);
        projectileData.CollisionSize = hitSize;
        projectileData.RatePenetration = ratePenetration;
        projectileData.FixedPenetration = fixedPenetration;
        projectileData.ProjectileSpeed = projectileSpeed;
        projectileData.RemainTime = duration;
    }

    public virtual void TestBalance()
    {
        var planet = transform.parent.GetComponentInChildren<Planet>();
        if (planet == null)
            return;

        if (currentTowerAttackId == towerAttackId || towerAttackId == -1)
            return;

        planet.RemoveTowerAt(0);

        planet.SetAttackTower(towerDataSOs[towerAttackId],0);

        towerAttack = planet.GetAttackTowerToAmpTower(0);

        projectileData = towerAttack.BaseProjectileData;
        if (projectileData == null)
            return;

        damage = projectileData.Attack;
        attackSpeed = towerAttack.AttackTowerData.fireRate;
        accuracy = towerAttack.AttackTowerData.Accuracy;
        grouping = towerAttack.AttackTowerData.grouping;
        projectileNum = towerAttack.BaseProjectileCount;
        targetNum = towerAttack.TargetingSystem.BaseTargetCount;
        hitSize = projectileData.CollisionSize;
        ratePenetration = projectileData.RatePenetration;
        fixedPenetration = projectileData.FixedPenetration;
        projectileSpeed = projectileData.ProjectileSpeed;
        duration = projectileData.RemainTime;

        currentTowerAttackId = towerAttackId;

        choosedTowerData = towerDataSOs[towerAttackId];
    }

    public void SetProjectileData()
    {
        if (towerAttack == null || projectileData == null)
            return;

        projectileData.Attack = damage;
        projectileData.CollisionSize = hitSize;
        projectileData.RatePenetration = ratePenetration;
        projectileData.FixedPenetration = fixedPenetration;
        projectileData.ProjectileSpeed = projectileSpeed;
        projectileData.RemainTime = duration;

        DataTableManager.ProjectileTable.Set(projectileData.Projectile_ID, projectileData);
    }
}
