using UnityEngine;

public class ExplosionAbility : EffectAbility
{
    private GameObject explosionEffect;
    private ProjectileData projectileData;
    private Projectile cachedProjectile;
    public ExplosionAbility(float amount)
    {
        upgradeAmount = amount / 100f;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if(projectile != null)
        {
            cachedProjectile = projectile;
            projectileData = projectile.projectileData;
            upgradeAmount = projectile.explosionRadius / 100f;
        }

        var enemy = gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            float explosionDamageMul = GetExplosionDamageMultiplier();

            Explosion explosion = null;
            if (ExplosionPoolManager.Instance != null)
            {
                explosion = ExplosionPoolManager.Instance.Get();
            }
            else
            {
                var obj = LoadManager.GetLoadedGamePrefab(ObjectName.Explosion);
                explosion = obj != null ? obj.GetComponent<Explosion>() : null;
            }

            if (explosion != null)
            {
                explosion.transform.position = enemy.transform.position;
                explosion.SetInit(0.01f, upgradeAmount, projectileData, cachedProjectile, explosionDamageMul);
            }
        }

    }

    public override void RemoveAbility(GameObject gameObject)
    {
        base.RemoveAbility(gameObject);
    }

    public override void Setting(GameObject gameObject)
    {
        base.Setting(gameObject);
    }

    public override string ToString()
    {
        return $"Explosion\nAbility!!";
    }

    public override IAbility Copy()
    {
        return new ExplosionAbility(upgradeAmount * 100);
    }
    private float GetExplosionDamageMultiplier()
    {
        const int EXPLOSION_ABILITY_ID = 200008;

        if (cachedProjectile == null)
            return 0.1f; 

        if (cachedProjectile.towerAbilities == null ||
            !cachedProjectile.towerAbilities.Contains(EXPLOSION_ABILITY_ID))
            return 0.1f;

        if (!DataTableManager.IsInitialized)
            return 0.1f;

        var ra = DataTableManager.RandomAbilityTable?.Get(EXPLOSION_ABILITY_ID);
        if (ra == null || ra.RandomAbilityType != 1)
            return 0.1f;

        if (TowerReinforceManager.Instance == null)
            return 0.1f;

        return TowerReinforceManager.Instance
            .GetFinalSuperValueForAbility(
                EXPLOSION_ABILITY_ID,
                cachedProjectile.towerReinforceLevel
            );
    }
}