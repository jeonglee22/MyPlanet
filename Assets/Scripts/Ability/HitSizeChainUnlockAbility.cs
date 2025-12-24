public class HitSizeChainUnlockAbility : UnlockMultiEffectAbilityBase
{
    public HitSizeChainUnlockAbility() : base(200023) { }

    public override IAbility Copy()
    {
        return new HitSizeChainUnlockAbility();
    }
}
