public class AccuracyHomingUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AccuracyHomingUnlockAbility() : base(200021) { }

    public override IAbility Copy()
    {
        return new AccuracyHomingUnlockAbility();
    }
}