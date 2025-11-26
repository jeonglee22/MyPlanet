using UnityEngine;

public class MeteorClusterSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    public MeteorClusterSummonPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Summon()
    {
        switch (owner.Data.EnemyGrade)
        {
            case 1:
                owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, summonData.EnemyQuantity_1, owner.ScaleData, false, owner.transform.position);
                break;
            case 2:
                var spawner = GetRandomSpawner();
                spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, summonData.EnemyQuantity_1, owner.ScaleData, false);
                break;
        }
    }

    public override void PatternUpdate()
    {

    }
}
