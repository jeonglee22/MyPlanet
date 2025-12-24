public class AtkProjSpeedUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AtkProjSpeedUnlockAbility() : base(200020) { }

    public override IAbility Copy()
    {
        return new AtkProjSpeedUnlockAbility();
    }
}
