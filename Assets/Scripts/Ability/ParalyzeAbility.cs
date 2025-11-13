using System.Text;
using UnityEngine;

public class ParalyzeAbility : PassiveAbility
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
        if (movement != null)
        {
            initSpeed = movement.moveSpeed;
            movement.moveSpeed = 0;
            movement.isDebuff = true;
            Debug.Log("Paralyze Apply");
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
