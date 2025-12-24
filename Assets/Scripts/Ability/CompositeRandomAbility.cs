using System.Collections.Generic;
using UnityEngine;

public class CompositeRandomAbility : IAbility
{
    private List<IAbility> subAbilities = new List<IAbility>();
    private int abilityId;
    private float upgradeAmount;

    public float UpgradeAmount => upgradeAmount;
    public AbilityApplyType AbilityType => AbilityApplyType.None;

    public CompositeRandomAbility(RandomAbilityData data)
    {
        if (data == null) return;

        abilityId = data.RandomAbility_ID;
        upgradeAmount = 0f;

        var effectToAbility = CreateEffectToAbilityMap();

        if (data.SpecialEffect_ID != 0 && effectToAbility.ContainsKey(data.SpecialEffect_ID))
        {
            var ability = effectToAbility[data.SpecialEffect_ID](data.SpecialEffectValue);
            if (ability != null) subAbilities.Add(ability);
        }

        if (data.SpecialEffect2_ID.HasValue && data.SpecialEffect2_ID.Value != 0 &&
            effectToAbility.ContainsKey(data.SpecialEffect2_ID.Value))
        {
            var ability = effectToAbility[data.SpecialEffect2_ID.Value](data.SpecialEffect2Value ?? 0f);
            if (ability != null) subAbilities.Add(ability);
        }

        if (data.SpecialEffect3_ID.HasValue && data.SpecialEffect3_ID.Value != 0 &&
            effectToAbility.ContainsKey(data.SpecialEffect3_ID.Value))
        {
            var ability = effectToAbility[data.SpecialEffect3_ID.Value](data.SpecialEffect3Value ?? 0f);
            if (ability != null) subAbilities.Add(ability);
        }
    }

    private Dictionary<int, System.Func<float, IAbility>> CreateEffectToAbilityMap()
    {
        return new Dictionary<int, System.Func<float, IAbility>>
        {
            { 1011001, v => new AttackSpeedAbility(v) },
            { 1102001, v => new AttackUpgradeAbility(v) },
            { 1102002, v => new ProjectileSpeedAbility(v) },
            { 1102004, v => new HItSizeUpgradeAbility(v) },
            { 1101001, v => new PierceUpgradeAbility(v) },
            { 1101002, v => new ChainUpgradeAbility(v) },
            { 1101003, v => new ExplosionAbility(v) },
            { 1101004, v => new HomingUpgradeAbility(v) },
            { 1105001, v => new AccuracyAbility(v) },
        };
    }

    public void ApplyAbility(GameObject gameObject)
    {
        foreach (var ability in subAbilities)
        {
            ability.ApplyAbility(gameObject);
        }
    }

    public void RemoveAbility(GameObject gameObject)
    {
        foreach (var ability in subAbilities)
        {
            ability.RemoveAbility(gameObject);
        }
    }

    public void Setting(GameObject gameObject)
    {
        foreach (var ability in subAbilities)
        {
            ability.Setting(gameObject);
        }
    }

    public void StackAbility(float amount) { }

    public IAbility Copy()
    {
        var data = DataTableManager.RandomAbilityTable.Get(abilityId);
        return new CompositeRandomAbility(data);
    }
}