using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainUpgradeAbility : EffectAbility
{
    private ProjectileData projectileData;
    private ProjectileData baseData;
    private ProjectileData buffedData;
    private bool isSetup = false;
    private List<Enemy> hitEnemies;

    private Projectile cachedProjectile;
    private float chainDamageMultiplier;

    public ChainUpgradeAbility(float amount)
    {
        upgradeAmount = amount;
        abilityType = AbilityApplyType.Fixed;
        hitEnemies = new List<Enemy>();
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var projectile = gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            SetProjectile(projectile);
            projectileData = projectile.projectileData;
        }
        var enemy = gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            hitEnemies.Add(enemy);
            ChainingDamage(enemy, Mathf.FloorToInt(upgradeAmount));
            
            var chain = LoadManager.GetLoadedGamePrefab(ObjectName.ChainEffect);
            var lineRenderer =  chain.GetComponent<LineRenderer>();
            if(lineRenderer == null)
                return;
            
            lineRenderer.positionCount = hitEnemies.Count;
            for (int i = 0; i < hitEnemies.Count; i++)
            {
                lineRenderer.SetPosition(i, hitEnemies[i].transform.position);
            }
            hitEnemies.Clear();
        }
    }

    private void ChainingDamage(Enemy enemy, int count)
    {
        if (enemy == null || count <= 0 || projectileData == null)
            return;

        var nearboundEnemyColliders = Physics.OverlapSphere(enemy.transform.position, 2f);
        if (nearboundEnemyColliders.Length == 0)
            return;

        var nextEnemies = nearboundEnemyColliders.ToList()
            .ConvertAll(x => x.GetComponent<Enemy>())
            .FindAll(x => x != null && !hitEnemies.Contains(x));

        if (nextEnemies.Count == 0)
            return;

        var index = Random.Range(0, nextEnemies.Count);
        var nextEnemy = nextEnemies[index];
        if (nextEnemy == null)
        {
            return;
        }

        int chainLevel = Mathf.FloorToInt(upgradeAmount) - count + 1;
        float chainDecay = Mathf.Pow(chainDamageMultiplier, chainLevel);
        projectileData.Attack = buffedData.Attack * chainDecay;

        var damage = CalculateTotalDamage(nextEnemy.Data.Defense);
        nextEnemy.OnDamage(damage);
        cachedProjectile.ActionEvent(damage);
        hitEnemies.Add(nextEnemy);
        ChainingDamage(nextEnemy, count - 1);
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
        return $"Chain\nAttack!!";
    }

    private void SetProjectile(Projectile projectile)
    {
        if (projectile != null)
        {
            this.cachedProjectile = projectile;
            baseData = projectile.BaseData;
            buffedData = projectile.projectileData;
            chainDamageMultiplier = GetChainDamageMultiplier();
        }
    }

    public override IAbility Copy()
    {
        return new ChainUpgradeAbility(upgradeAmount);
    }

    public float CalculateTotalDamage(float enemyDef)
    {
        var RatePanetration = Mathf.Clamp(projectileData.RatePenetration, 0f, 100f);

        var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - projectileData.FixedPenetration;
        if (totalEnemyDef < 0)
        {
            totalEnemyDef = 0;
        }

        var totalDamage = projectileData.Attack * 100f / (100f + totalEnemyDef);

        return totalDamage;
    }

    private float GetChainDamageMultiplier()
    {
        const int CHAIN_ABILITY_ID = 200007;

        if (cachedProjectile == null)
            return 0.2f; 

        if (cachedProjectile.towerAbilities == null ||
            !cachedProjectile.towerAbilities.Contains(CHAIN_ABILITY_ID))
            return 0.2f;

        if (!DataTableManager.IsInitialized)
            return 0.2f;

        var ra = DataTableManager.RandomAbilityTable?.Get(CHAIN_ABILITY_ID);
        if (ra == null || ra.RandomAbilityType != 1)
            return 0.2f;

        if (TowerReinforceManager.Instance == null)
            return 0.2f;

        return TowerReinforceManager.Instance
            .GetFinalSuperValueForAbility(
                CHAIN_ABILITY_ID,
                cachedProjectile.towerReinforceLevel
            );
    }
}