using UnityEngine;

public class SimpleSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;
    
    private bool isFirstSummon = false;

    public SimpleSummonPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Summon()
    {
        if(owner?.Spawner == null || summonData == null)
        {
            return;
        }

        //(int enemyId, int quantity, ScaleData scaleData, int moveType, bool ShouldDropItems = true, Vector3 spawnPos = default, Enemy parent = null)
        if(PatternIds.BigFireEyeSummonFireChild == (PatternIds)PatternId)
        {
            int index = Random.Range(2, 4);
            owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, summonData.EnemyQuantity_1, owner.ScaleData, summonEnemyData.MoveType, false, SpawnManager.Instance.GetSpawner(index).transform.position, owner);
            return;
        }
        
        if(PatternIds.WhiteHoleSpaceWarmSummon == (PatternIds)PatternId)
        {
            if (!isFirstSummon)
            {
                owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, summonData.EnemyQuantity_1, owner.ScaleData, summonEnemyData.MoveType, false, owner.transform.position, owner);
                isFirstSummon = true;   
            }
            
            return;
        }

        owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, summonData.EnemyQuantity_1, owner.ScaleData, summonEnemyData.MoveType, false, owner.transform.position, owner);
        
    }
}
