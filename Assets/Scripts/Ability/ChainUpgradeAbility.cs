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
        }
    }

    private void ChainingDamage(Enemy enemy, int count)
    {
        if(enemy == null || count <= 0 || projectileData == null)
            return;
        
        var nearboundEnemyColliders = Physics.OverlapSphere(enemy.transform.position, 2f);
        if(nearboundEnemyColliders.Length == 0)
            return;

        var nextEnemies = nearboundEnemyColliders.ToList().ConvertAll(x => x.GetComponent<Enemy>()).FindAll(x => !hitEnemies.Contains(x));
        var index = Random.Range(0, nextEnemies.Count);
        var nextEnemy = nextEnemies[index];
        if (nextEnemy == null)
        {
            return;
        }

        projectileData.Attack = buffedData.Attack * Mathf.Pow(0.9f, (upgradeAmount - count));
        nextEnemy.OnDamage(CalculateTotalDamage(enemy.Data.Defense));
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
            baseData = projectile.BaseData;
            buffedData = projectile.projectileData;
        }
    }

    public override IAbility Copy()
    {
        return new ChainUpgradeAbility(upgradeAmount);
    }

    public float CalculateTotalDamage(float enemyDef)
    {
        var RatePanetration = Mathf.Clamp(projectileData.RatePenetration, 0f, 100f);
        // Debug.Log(damage);
        var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - projectileData.FixedPenetration;
        if(totalEnemyDef < 0)
        {
            totalEnemyDef = 0;
        }
        var totalDamage = projectileData.Attack * 100f / (100f + totalEnemyDef);
        
        return totalDamage;
    }
}