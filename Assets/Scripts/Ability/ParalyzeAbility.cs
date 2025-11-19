using System.Text;
using UnityEngine;

public class ParalyzeAbility : EffectAbility
{
    private float initSpeed;

    public ParalyzeAbility()
    {
        upgradeAmount = 0f;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var movement = gameObject.GetComponent<EnemyMovement>();
        var enemy = gameObject.GetComponent<Enemy>();
        if (movement != null)
        {
            initSpeed = movement.moveSpeed;
            movement.moveSpeed = 0;
            movement.isDebuff = true;

            /*
            if (enemy.Data.hitEffect != null)
            {
                var effectInstance = GameObject.Instantiate(enemy.Data.hitEffect, enemy.transform.position, Quaternion.identity);
                effectInstance.Play();
                
                GameObject.Destroy(effectInstance.gameObject, effectInstance.main.duration + effectInstance.main.startLifetime.constantMax);
            }
            */
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
        var sb = new StringBuilder();
        sb.Append($"Paralyze\nAttack!!");

        return sb.ToString();
    }
}
