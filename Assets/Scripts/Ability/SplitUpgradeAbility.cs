using System;
using UnityEngine;

public class SplitUpgradeAbility : EffectAbility
{
    private TowerAttack towerAttack;
    private ProjectileData baseData;
    private ProjectileData buffedData;
    private Vector3 direction;
    private Transform firePoint;
    private float offsetAngle = 30f;
    private ProjectilePoolManager projectilePoolManager;
    private bool isSetup = false;

    private int pierceCount;
    private Projectile projectile;
    private int splitCount;
    private bool isLazerSplit;
    private bool isSetupLazer = false;
    private LazertowerAttack lazer;

    public SplitUpgradeAbility(float amount)
    {
        upgradeAmount = amount;
        abilityType = AbilityApplyType.Fixed;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            SetProjectile(projectile);
        }
        var lazer = gameObject.GetComponent<LazertowerAttack>();
        if (lazer != null)
        {
            this.lazer = lazer;
        }
        var enemy = gameObject.GetComponent<Enemy>();
        if (enemy != null && isSetup && this.projectile.splitCount > 0)
        {
            if (isLazerSplit)
            {
                if (isSetupLazer)
                    return;

                MakeLazerSplit(this.projectile.splitCount, this.lazer.TailTransform);
                isSetupLazer = true;
                return;
            }
            MakeSplit(offsetAngle);
            MakeSplit(-offsetAngle);
            isSetup = false;
            this.projectile.splitCount = 0;
        }
    }

    private void MakeLazerSplit(int splitCount, Transform target)
    {
        var splitLines = splitCount + 1;
        var splitAngle = 90f;

        float[] eachAngles = new float[splitLines];
        for (int i = 0; i < splitLines; i++)
        {
            eachAngles[i] = -splitAngle / 2 + splitAngle / (splitLines - 1) * i;
        }
        float splitMul = GetSplitDamageMultiplier();

        foreach (var angle in eachAngles)
        {
            var direction = Quaternion.Euler(0, 0, angle) * this.direction;
            var newProjectiles = projectilePoolManager.GetProjectile(baseData);

            var lazerObj = LoadManager.GetLoadedGamePrefab(ObjectName.Lazer);
            var lazer = lazerObj.GetComponent<LazertowerAttack>();

            newProjectiles.Initialize(
                buffedData,
                baseData,
                direction,
                true,
                ProjectilePoolManager.Instance.ProjectilePool,
                null,
                this.projectile.towerReinforceLevel,
                this.projectile.towerAbilities
            );
            newProjectiles.damageMultiplier = splitMul;

            lazer.SetLazer(target, angle, null, newProjectiles, towerAttack, projectile.projectileData.RemainTime, true, this.lazer);

            newProjectiles.gameObject.SetActive(false);
            this.lazer.IsSplitSet = true;
        }
    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);

        towerAttack = gameObject.GetComponent<TowerAttack>();
        projectilePoolManager = ProjectilePoolManager.Instance;

        if (towerAttack.AttackTowerData.towerIdInt == (int)AttackTowerId.Lazer)
        {
            isLazerSplit = true;
            isSetupLazer = false;
        }
    }

    public override string ToString()
    {
        return $"Split\nAttack!!";
    }

    private void SetProjectile(Projectile projectile)
    {
        if (projectile != null)
        {
            baseData = projectile.BaseData;
            buffedData = projectile.projectileData;
            direction = projectile.direction;
            firePoint = projectile.transform;
            this.projectile = projectile;
            this.projectile.splitCount = (int)upgradeAmount;
            isSetup = true;
        }
    }

    private void MakeSplit(float offset)
    {
        var newProjectiles = ProjectilePoolManager.Instance.GetProjectile(baseData);
        var newDirection = Quaternion.Euler(0, 0, offset) * direction;

        newProjectiles.transform.position = firePoint.position + newDirection * 0.3f;
        newProjectiles.transform.rotation = Quaternion.LookRotation(newDirection);

        //Initialize Buffed Data
        newProjectiles.Initialize(
            buffedData,
            baseData,
            newDirection,
            true,
            ProjectilePoolManager.Instance.ProjectilePool,
            null,
            this.projectile.towerReinforceLevel,
            this.projectile.towerAbilities
            );

        float splitMul = GetSplitDamageMultiplier();
        newProjectiles.damageMultiplier = splitMul;
        newProjectiles.damageEvent += towerAttack.AdddamageDealt;
        newProjectiles.explosionRadius = this.projectile.explosionRadius;
        var abilities = towerAttack?.Abilities;

        foreach (var abilityId in abilities)
        {
            if(abilityId == (int)AbilityId.Split)
            {
                var remainingSplits = this.projectile.splitCount - 1;
                var splitAbility = new SplitUpgradeAbility(remainingSplits);
                splitAbility.Setting(towerAttack.gameObject);
                splitAbility.ApplyAbility(newProjectiles.gameObject);
                newProjectiles.abilityAction += splitAbility.ApplyAbility;
                newProjectiles.abilityRelease += splitAbility.RemoveAbility;
                continue;
            }
            var ability = AbilityManager.GetAbility(abilityId);
            ability.ApplyAbility(newProjectiles.gameObject);
            newProjectiles.abilityAction += ability.ApplyAbility;
            newProjectiles.abilityRelease += ability.RemoveAbility;
        }
        newProjectiles.currentPierceCount = 1;
    }

    public override IAbility Copy()
    {
        return new SplitUpgradeAbility(upgradeAmount);
    }

    private float GetSplitDamageMultiplier()
    {
        const int SPLIT_ABILITY_ID = 200010;

        if (this.projectile == null)
            return 0.2f;  

        if (this.projectile.towerAbilities == null ||
            !this.projectile.towerAbilities.Contains(SPLIT_ABILITY_ID))
            return 0.2f;

        if (!DataTableManager.IsInitialized)
            return 0.2f;

        var ra = DataTableManager.RandomAbilityTable?.Get(SPLIT_ABILITY_ID);
        if (ra == null || ra.RandomAbilityType != 1)
            return 0.2f;

        if (TowerReinforceManager.Instance == null)
            return 0.2f;

        return TowerReinforceManager.Instance
            .GetFinalSuperValueForAbility(
                SPLIT_ABILITY_ID,
                this.projectile.towerReinforceLevel
            );
    }
}