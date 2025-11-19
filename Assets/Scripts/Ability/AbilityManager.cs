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
        abilityDict.Add(3, new AttackSpeedAbility());

        abilityDict.Add(101, new ParalyzeAbility());
    }

    public int GetRandomAbility()
    {
        var count = abilityDict.Count;

        if(count == 0)
            return -1;

        var index = Random.Range(0, count);
        var keys = new List<int>(abilityDict.Keys);
        return keys[index];
    }

    public IAbility GetAbility(int id)
    {
        if (abilityDict.ContainsKey(id))
            return abilityDict[id];
        
        return null;
    }
}
