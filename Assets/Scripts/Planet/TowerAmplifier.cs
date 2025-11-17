using System.Collections.Generic;
using UnityEngine;

public class TowerAmplifier : MonoBehaviour
{
    [SerializeField] private AmplifierTowerDataSO amplifierTowerData;
    public AmplifierTowerDataSO AmplifierTowerData => amplifierTowerData;

    private readonly List<TowerAttack> buffedTargets = new List<TowerAttack>();

    public void ApplyBuff(TowerAttack target) //single target(apply buff)
    {
        if (target == null) return;
        if (amplifierTowerData == null) return;

        target.SetUpBuff(amplifierTowerData); //allow overlap
        
        if(!buffedTargets.Contains(target)) //detect targets
        {
            buffedTargets.Add(target);
        }
    }

    public void RemoveBuff(TowerAttack target) //single target (destory target tower)
    {
        if (target == null) return;
        if (!buffedTargets.Contains(target)) return;

        target.SetUpBuff(null);
        buffedTargets.Remove(target);
    }

    public void ClearAllbuffs()//(Destory Buff Tower)
    { 
        foreach(var t in buffedTargets)
        {
            if (t == null) continue;
            t.SetUpBuff(null);
        }
        buffedTargets.Clear();
    }

    private void OnDestroy()
    {
        ClearAllbuffs();
    }
}
