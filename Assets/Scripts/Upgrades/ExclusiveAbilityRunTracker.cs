using System.Collections.Generic;

public static class ExclusiveAbilityRunTracker
{
    private static readonly HashSet<int> taken = new HashSet<int>();
    public static bool IsTaken(int abilityId) => abilityId > 0 && taken.Contains(abilityId);
    public static void MarkTaken(int abilityId)
    {
        if (abilityId > 0) taken.Add(abilityId);
    }
    public static void Clear() => taken.Clear();
}