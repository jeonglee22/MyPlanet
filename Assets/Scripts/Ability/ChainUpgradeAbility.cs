using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainUpgradeAbility : EffectAbility
{
    private Projectile projectile;
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
            this.projectile = projectile;
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
        if(enemy == null || count <= 0 || projectile == null)
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

        projectile.damage = buffedData.Attack * Mathf.Pow(0.9f, (upgradeAmount - count));
        nextEnemy.OnDamage(projectile.CalculateTotalDamage(enemy.Data.Defense));
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
}
