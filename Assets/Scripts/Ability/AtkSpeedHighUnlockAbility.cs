public class AtkSpeedHighUnlockAbility : UnlockMultiEffectAbilityBase
{
    public AtkSpeedHighUnlockAbility() : base(200019) { }

    public override IAbility Copy()
    {
        return new AtkSpeedHighUnlockAbility();
    }
}