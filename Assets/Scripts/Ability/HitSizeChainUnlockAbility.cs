using UnityEngine;

public class HitSizeChainUnlockAbility : UnlockMultiEffectAbilityBase
{
    public HitSizeChainUnlockAbility() : base(200023) { }

    protected override UnlockMultiEffectAbilityBase CreateNewInstance()
    {
        return new HitSizeChainUnlockAbility();
    }

    public override IAbility Copy()
    {
        return CopyInternal();
    }
}