using UnityEngine;

public class ReinforceAbilityFactory
{
    public static IAbility Create(int abilityId, int reinforceLevel)
    {
        var inst = AbilityManager.GetAbility(abilityId);
        if (inst == null) return null;

        if (reinforceLevel <= 0) return inst;

        var ra = DataTableManager.RandomAbilityTable.Get(abilityId);
        if (ra == null) return inst;

        float rawBase = ra.SpecialEffectValue;
        float rawFinal = TowerReinforceManager.Instance
            .GetFinalPrimaryValueForAbility(abilityId, reinforceLevel);

        float rawAdd = rawFinal - rawBase;
        if (Mathf.Approximately(rawAdd, 0f)) return inst;

        float factor = 1f;
        if (!Mathf.Approximately(rawBase, 0f))
        {
            factor = inst.UpgradeAmount / rawBase; 
        }
        else
        {
            factor = (inst.AbilityType == AbilityApplyType.Rate) ? 0.01f : 1f;
        }

        float internalAdd = rawAdd * factor;
        if (!Mathf.Approximately(internalAdd, 0f))
            inst.StackAbility(internalAdd);

        return inst;
    }

    public static float GetFinalSuper(int abilityId, int reinforceLevel)
    {
        return TowerReinforceManager.Instance.GetFinalSuperValueForAbility(abilityId, reinforceLevel);
    }
}
