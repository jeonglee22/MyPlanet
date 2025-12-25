using System.Collections.Generic;
using UnityEngine;

public static class RandomAbilityNameUtil
{
    public static string BuildMultiLineName(int abilityId)
    {
        if (!DataTableManager.IsInitialized) return string.Empty;

        var row = DataTableManager.RandomAbilityTable.Get(abilityId);
        if (row == null) return string.Empty;

        var names = new List<string>(3);

        if (!string.IsNullOrWhiteSpace(row.RandomAbilityName))
            names.Add(row.RandomAbilityName);

        if (!string.IsNullOrWhiteSpace(row.RandomAbility2Name))
            names.Add(row.RandomAbility2Name);

        if (!string.IsNullOrWhiteSpace(row.RandomAbility3Name))
            names.Add(row.RandomAbility3Name);

        if (names.Count == 0) return row.RandomAbilityName ?? string.Empty;

        return string.Join("\n", names);
    }
}