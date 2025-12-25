using UnityEngine;

public class AccuracyHomingUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AccuracyHomingUnlockAbility() : base(200021) { }

    protected override UnlockMultiEffectAbilityBase CreateNewInstance()
    {
        return new AccuracyHomingUnlockAbility();
    }

    public override IAbility Copy()
    {
        return CopyInternal();
    }
}