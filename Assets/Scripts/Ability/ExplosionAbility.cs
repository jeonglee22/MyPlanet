using UnityEngine;

public class ExplosionAbility : EffectAbility
{
    private GameObject explosionEffect;
    private Projectile projectile;

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
            this.projectile = projectile;
        }

        var enemy = gameObject.GetComponent<Enemy>();
        if(enemy != null)
        {
            var obj = LoadManager.GetLoadedGamePrefab(ObjectName.Explosion);
            obj.transform.position = enemy.transform.position;
            var explosion = obj.GetComponent<Explosion>();
            explosion.SetInit(0.01f, upgradeAmount, this.projectile);
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
}
