public class AtkSpeedAtkHitSizeUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AtkSpeedAtkHitSizeUnlockAbility() : base(200018) { }

    public override IAbility Copy()
    {
        return new AtkSpeedAtkHitSizeUnlockAbility();
    }
}