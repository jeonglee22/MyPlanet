using UnityEngine;

public class ExplosionAbility : EffectAbility
{
    private GameObject explosionEffect;

    public ExplosionAbility()
    {
        upgradeAmount = 0.1f;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

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
            explosion.SetInitRadius(0.01f, upgradeAmount);
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
}
