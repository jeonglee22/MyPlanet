using UnityEngine;

public class AtkSpeedHighUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AtkSpeedHighUnlockAbility() : base(200019) { }

    protected override UnlockMultiEffectAbilityBase CreateNewInstance()
    {
        return new AtkSpeedHighUnlockAbility();
    }

    public override IAbility Copy()
    {
        return CopyInternal();
    }
}