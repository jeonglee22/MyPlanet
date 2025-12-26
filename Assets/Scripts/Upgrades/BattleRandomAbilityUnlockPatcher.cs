using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class BattleRandomAbilityUnlockPatcher
{
    private const int MaxReinforceLevel = 4;
    private static bool appliedThisBattle = false;

    public static async UniTask ApplyOnceAsync()
    {
        if (appliedThisBattle) return;

        await UniTask.WaitUntil(() => DataTableManager.IsInitialized);
        await UniTask.WaitUntil(() =>
            UserTowerUpgradeManager.Instance != null &&
            UserTowerUpgradeManager.Instance.IsInitialized
        );

        var waitData = UniTask.WaitUntil(() =>
            UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData != null
        );
        var timeout = UniTask.Delay(TimeSpan.FromSeconds(1));
        int winner = await UniTask.WhenAny(waitData, timeout);

        if (winner != 0)
        {
            Debug.LogWarning("[UnlockPatch] CurrentTowerUpgradeData not ready. Skip patch this battle.");
            return;
        }

        var userData = UserTowerUpgradeManager.Instance.CurrentTowerUpgradeData;
        if (userData == null) return;

        int count = Mathf.Min(userData.towerIds?.Count ?? 0, userData.upgradeLevels?.Count ?? 0);
        if (count <= 0)
        {
            appliedThisBattle = true;
            return;
        }

        int totalChecked = 0;
        int newlyAdded = 0;
        int alreadyExists = 0;

        for (int i = 0; i < count; i++)
        {
            int towerId = userData.towerIds[i];
            int level = userData.upgradeLevels[i];

            if (level < MaxReinforceLevel) continue;

            totalChecked++;

            int dataId = DataTableManager.TowerUpgradeAbilityUnlockTable.GetDataId(towerId);
            if (dataId < 0) continue;

            var unlockRow = DataTableManager.TowerUpgradeAbilityUnlockTable.Get(dataId);
            if (unlockRow == null) continue;

            int groupId = unlockRow.RandomAbilityGroup_ID;
            int abilityId = unlockRow.RandomAbility_ID;

            if (groupId <= 0 || abilityId <= 0) continue;

            var groupData = DataTableManager.RandomAbilityGroupTable.Get(groupId);
            if (groupData == null || groupData.RandomAbilityGroupList == null)
                continue;

            if (!groupData.RandomAbilityGroupList.Contains(abilityId))
            {
                groupData.RandomAbilityGroupList.Add(abilityId);
                newlyAdded++;
                Debug.Log($"[UnlockPatch] Tower {towerId}: Added ability={abilityId} to group={groupId}");
            }
            else
            {
                alreadyExists++;
                Debug.Log($"[UnlockPatch] Tower {towerId}: Ability={abilityId} already in group={groupId} (skip)");
            }
        }

        appliedThisBattle = true;

        if (totalChecked == 0)
        {
            Debug.Log("[UnlockPatch] No level 4 towers found.");
        }
        else
        {
            Debug.Log($"[UnlockPatch] Complete: {totalChecked} towers checked, " +
                      $"{newlyAdded} abilities added, {alreadyExists} already existed.");
        }
    }

    public static void ResetForNewBattle()
    {
        appliedThisBattle = false;
    }
}