using System.Text;
using UnityEngine;

public class ParalyzeAbility : EffectAbility
{
    private float initSpeed;
    public float slowPercentage = 40f;

    public ParalyzeAbility(float amount)
    {
        upgradeAmount = amount;
        abilityType = AbilityApplyType.Rate;
    }

    public override void ApplyAbility(GameObject gameObject)
    {
        base.ApplyAbility(gameObject);

        var movement = gameObject.GetComponent<EnemyMovement>();
        var enemy = gameObject.GetComponent<Enemy>();
        if (movement != null)
        {
            initSpeed = movement.moveSpeed;

            movement.moveSpeed = initSpeed * (1f - upgradeAmount / 100f);
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

    public override IAbility Copy()
    {
        return new ParalyzeAbility(upgradeAmount);
    }
}
