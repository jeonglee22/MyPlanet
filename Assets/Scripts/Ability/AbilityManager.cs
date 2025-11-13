using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    private Dictionary<int, IAbility> abilityDict;
    public Dictionary<int, IAbility> AbilityDict => abilityDict;

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
        abilityDict.Add(1, new AccelationUpgradeAbility());
        abilityDict.Add(2, new SpeedUpgradeAbility());
    }

    public IAbility GetRandomAbility()
    {
        var count = abilityDict.Count;

        if(count == 0)
            return null;

        var id = Random.Range(0, count);
        return abilityDict[id];
    }
}
