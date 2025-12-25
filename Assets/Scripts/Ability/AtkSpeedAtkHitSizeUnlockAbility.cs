using UnityEngine;

public class AtkSpeedAtkHitSizeUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AtkSpeedAtkHitSizeUnlockAbility() : base(200018) { }

    protected override UnlockMultiEffectAbilityBase CreateNewInstance()
    {
        return new AtkSpeedAtkHitSizeUnlockAbility();
    }

    public override IAbility Copy()
    {
        return CopyInternal();
    }
}