using UnityEngine;

public class ExplosionRangePierceUnlockAbility : UnlockMultiEffectAbilityBase
{
    public ExplosionRangePierceUnlockAbility() : base(200022) { }

    protected override UnlockMultiEffectAbilityBase CreateNewInstance()
    {
        return new ExplosionRangePierceUnlockAbility();
    }

    public override IAbility Copy()
    {
        return CopyInternal();
    }
}