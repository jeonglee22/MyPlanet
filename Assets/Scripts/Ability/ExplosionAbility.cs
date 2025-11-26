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
            var newGo = new GameObject("ExplosionEffect");

            newGo.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            var renderer = newGo.AddComponent<MeshRenderer>();
            renderer.material.color = new Color(0f, 1f, 0f, 0.2f);

            newGo.transform.position = gameObject.transform.position;
            newGo.AddComponent<SphereCollider>().isTrigger = true;

            var explosion = newGo.AddComponent<Explosion>();
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
