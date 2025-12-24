public class ExplosionRangePierceUnlockAbility : UnlockMultiEffectAbilityBase
{
    public ExplosionRangePierceUnlockAbility() : base(200022) { }

    public override IAbility Copy()
    {
        return new ExplosionRangePierceUnlockAbility();
    }
}
