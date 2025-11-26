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
        owner.Spawner.SpawnEnemiesWithMovement(summonData.Enemy_Id, summonData.EnemyQuantity_1, owner.ScaleData, summonEnemyData.MoveType, false);
    }

    public override void PatternUpdate()
    {

    }
}
