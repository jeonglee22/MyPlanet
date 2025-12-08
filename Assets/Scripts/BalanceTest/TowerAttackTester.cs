using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Unity.VisualScripting;
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

    [SerializeField] private TowerDataSO[] towerDataSOs;
    [SerializeField] private TargetRangeSO[] targetRangeSOs;
    private ProjectileData projectileData;

    public float explosionRadius;
    public int homing;
    public int splitCount;
    public float slowPercent;
    public int chainCount;
    public int pierceCount;

    private List<IAbility> abilities;

    private async UniTaskVoid Start()
    {
        Initialize();

        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        abilities = new List<IAbility>();
        abilities.Add(new HomingUpgradeAbility(0f));
        abilities.Add(new ExplosionAbility(0f));
        abilities.Add(new SplitUpgradeAbility(0f));
        abilities.Add(new ParalyzeAbility(0f));
        abilities.Add(new ChainUpgradeAbility(0f));
        abilities.Add(new PierceUpgradeAbility(0f));
    }

    private void Initialize()
    {
        towerAttackId = -1;
        targetRange = -1;
        damage = 0f;
        attackSpeed = 0f;
        accuracy = 50f;
        grouping = 0f;
        projectileNum = 1;
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

        if (towerAttack.Abilities.Count == 0)
        {
            towerAttack.AddAbility((int)AbilityId.Homing);
            towerAttack.AddAbility((int)AbilityId.Explosion);
            towerAttack.AddAbility((int)AbilityId.Split);
            towerAttack.AddAbility((int)AbilityId.Slow);
            towerAttack.AddAbility((int)AbilityId.Chain);
            towerAttack.AddAbility((int)AbilityId.Pierce);
            towerAttack.TestAbilities = abilities;
        }

        for (int i = 0; i < abilities.Count; i++)
        {
            switch (abilities[i])
            {
                case HomingUpgradeAbility:
                    if (homing == 1)
                        abilities[i].ApplyAbility(towerAttack.gameObject);
                    else
                        abilities[i].RemoveAbility(towerAttack.gameObject);
                    break;
                case ExplosionAbility:
                    var newExplosionAbility = new ExplosionAbility(explosionRadius);
                    abilities[i] = newExplosionAbility;
                    break;
                case SplitUpgradeAbility:
                    var newSplitUpgradeAbility = new SplitUpgradeAbility(splitCount);
                    abilities[i] = newSplitUpgradeAbility;
                    break;
                case ParalyzeAbility:
                    var newParalyzeAbility = new ParalyzeAbility(slowPercent);
                    abilities[i] = newParalyzeAbility;
                    break;
                case ChainUpgradeAbility:
                    var newChainUpgradeAbility = new ChainUpgradeAbility(chainCount);
                    abilities[i] = newChainUpgradeAbility;
                    break;
                case PierceUpgradeAbility:
                    var newPierceUpgradeAbility = new PierceUpgradeAbility(pierceCount);
                    abilities[i] = newPierceUpgradeAbility;
                    break;
            }
        }

        towerAttack.TestAbilities = abilities;
    }

    private void UpdateProjectileData()
    {
        if (projectileData == null || towerAttack == null)
            return;

        projectileData.Attack = damage;
        towerAttack.AttackTowerData.fireRate = attackSpeed;
        towerAttack.AttackTowerData.Accuracy = accuracy;
        towerAttack.AttackTowerData.grouping = grouping;
        towerAttack.AttackTowerData.projectileCount = (int)projectileNum;
        towerAttack.TargetingSystem.SetExtraTargetCount(targetNum - 1);
        projectileData.CollisionSize = hitSize;
        projectileData.RatePenetration = ratePenetration;
        projectileData.FixedPenetration = fixedPenetration;
        projectileData.ProjectileSpeed = projectileSpeed;
        projectileData.RemainTime = duration;
    }

    protected virtual void TestBalance()
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
        projectileNum = towerAttack.AttackTowerData.projectileCount;
        targetNum = towerAttack.TargetingSystem.MaxTargetCount;
        hitSize = projectileData.CollisionSize;
        ratePenetration = projectileData.RatePenetration;
        fixedPenetration = projectileData.FixedPenetration;
        projectileSpeed = projectileData.ProjectileSpeed;
        duration = projectileData.RemainTime;

        currentTowerAttackId = towerAttackId;
    }
}
