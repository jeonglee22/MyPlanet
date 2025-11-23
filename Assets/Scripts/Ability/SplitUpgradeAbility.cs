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
        var enemy = gameObject.GetComponent<Enemy>();
        if (enemy != null && isSetup)
        {
            MakeSplit(offsetAngle);
            MakeSplit(-offsetAngle);
            isSetup = false;
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
            isSetup = true;
        }
    }

    private void MakeSplit(float offset)
    {
        var newProjectiles = ProjectilePoolManager.Instance.GetProjectile(baseData);
        var newDirection = Quaternion.Euler(0, 0, offset) * direction;

        newProjectiles.transform.position = firePoint.position;
        newProjectiles.transform.rotation = Quaternion.LookRotation(newDirection);

        //Initialize Buffed Data
        newProjectiles.Initialize(
            buffedData,
            baseData,
            newDirection,
            true,
            projectilePoolManager.ProjectilePool
            );

        var abilities = towerAttack?.Abilities;

        foreach (var abilityId in abilities)
        {
            if(abilityId == (int)AbilityId.Split)
                continue;

            var ability = AbilityManager.GetAbility(abilityId);

            ability.ApplyAbility(newProjectiles.gameObject);
            newProjectiles.abilityAction += ability.ApplyAbility;
            newProjectiles.abilityRelease += ability.RemoveAbility;
        }
    }

    public override IAbility Copy()
    {
        return new SplitUpgradeAbility(upgradeAmount);
    }
}
