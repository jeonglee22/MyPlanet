using UnityEngine;

/// <summary>
/// °ø¼Ó 30%
/// </summary>
public class UnlockAbility200019 : AttackSpeedAbility
{
    public UnlockAbility200019(float amount) : base(amount)
    {
    }
    public override IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(200019);
        return new UnlockAbility200019(data.SpecialEffectValue);
    }
}