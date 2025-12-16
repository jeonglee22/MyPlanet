using UnityEngine;

public class ReinforceAbilityFactory
{
    public static IAbility Create(int abilityId, int reinforceLevel)
    {
        var baseInstance = AbilityManager.GetAbility(abilityId);
        if (baseInstance == null) return null;
        if (reinforceLevel <= 0) return baseInstance;
        float finalPrimary = TowerReinforceManager.Instance.GetFinalPrimaryValueForAbility(abilityId, reinforceLevel);
        float delta = finalPrimary - baseInstance.UpgradeAmount;
        if (!Mathf.Approximately(delta, 0f))
            baseInstance.StackAbility(delta);
        return baseInstance;
    }
    public static float GetFinalSuper(int abilityId,int reinforceLevel)
    {
        return TowerReinforceManager.Instance.GetFinalSuperValueForAbility(abilityId, reinforceLevel);
    }
}