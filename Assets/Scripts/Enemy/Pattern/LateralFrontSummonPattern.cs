using UnityEngine;

public class LateralFrontSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float horizontalOffset = 1f;
    private float verticalOffset = 1f;

    public LateralFrontSummonPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Summon()
    {
        if(owner?.Spawner == null)
        {
            return;
        }

        int quantity = summonData.EnemyQuantity_1;
        int moveType = summonEnemyData.MoveType;

        Vector3[] spawnPositions = new Vector3[]
        {
            owner.transform.position + Vector3.left * horizontalOffset,
            owner.transform.position + Vector3.right * horizontalOffset,
            owner.transform.position + Vector3.down * verticalOffset
        };

        for(int i = 0; i < quantity; i++)
        {
            Vector3 spawnPos = spawnPositions[i % 3];

            owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, spawnPos, owner);
        }

        isExecuteOneTime = true;
    }
}
