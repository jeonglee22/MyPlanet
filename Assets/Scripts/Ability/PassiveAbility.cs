using UnityEngine;

public class PassiveAbility : IAbility
{
    protected TowerAttack tower;
    protected Projectile projectile;
    protected float amount;
    public float Amount => amount;

    public PassiveAbility()
    {
        tower = null;
        amount = 0f;
    }

    public PassiveAbility(PassiveAbility ability)
    {
        tower = ability.tower;
        amount = ability.amount;
    }

    public virtual void ApplyAbility(GameObject gameObject)
    {
    }

    public virtual void RemoveAbility(GameObject gameObject)
    {   
    }

    public virtual void Setting(GameObject gameObject)
    {
        if(gameObject is Projectile)
            projectile = gameObject.GetComponent<Projectile>();
    }
}
