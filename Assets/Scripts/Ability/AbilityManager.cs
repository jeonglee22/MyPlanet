using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public Dictionary<int, IAbility> abilityDict;

    private static AbilityManager instance;
    public static AbilityManager Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        abilityDict = new Dictionary<int, IAbility>();
        abilityDict.Add(0, new AttackUpgradeAbility());
    }
}
