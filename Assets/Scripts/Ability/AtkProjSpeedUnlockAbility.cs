using UnityEngine;

public class AtkProjSpeedUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AtkProjSpeedUnlockAbility() : base(200020) { }

    protected override UnlockMultiEffectAbilityBase CreateNewInstance()
    {
        return new AtkProjSpeedUnlockAbility();
    }

    public override IAbility Copy()
    {
        return CopyInternal();
    }
}