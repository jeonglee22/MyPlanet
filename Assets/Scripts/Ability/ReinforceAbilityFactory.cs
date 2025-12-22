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

        Debug.Log(
       $"[ReinforceAbilityFactory][Create] " +
       $"abilityId={abilityId}, reinforce={reinforceLevel}\n" +
       $"  rawBase(테이블 기본값)={rawBase}\n" +
       $"  rawFinal(GetFinalPrimary 반환)={rawFinal}\n" +
       $"  rawAdd(차이)={rawAdd}"
   );

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

        Debug.Log(
       $"[ReinforceAbilityFactory][Create] " +
       $"abilityId={abilityId}, reinforce={reinforceLevel}\n" +
       $"  inst.UpgradeAmount(현재)={inst.UpgradeAmount}\n" +
       $"  factor={factor}\n" +
       $"  internalAdd(추가할 값)={internalAdd}\n" +
       $"  최종 inst.UpgradeAmount(예상)={inst.UpgradeAmount + internalAdd}"
   );

        if (!Mathf.Approximately(internalAdd, 0f))
            inst.StackAbility(internalAdd);

        return inst;
    }

    public static float GetFinalSuper(int abilityId, int reinforceLevel)
    {
        return TowerReinforceManager.Instance.GetFinalSuperValueForAbility(abilityId, reinforceLevel);
    }
}
