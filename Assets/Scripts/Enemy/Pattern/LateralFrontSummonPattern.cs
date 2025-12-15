using UnityEngine;

public class LateralFrontSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float horizontalOffset = 3f;
    private float verticalOffset = 3f;

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

        Vector3[] spawnPositions = new Vector3[]
        {
            owner.transform.position + Vector3.left * horizontalOffset,
            owner.transform.position + Vector3.right * horizontalOffset,
            owner.transform.position + Vector3.down * verticalOffset
        };

        for(int i = 0; i < quantity; i++)
        {
            Vector3 spawnPos = spawnPositions[i % 3];

            Enemy childEnemy = owner.Spawner.SpawnEnemyAsChild(summonData.Enemy_Id, spawnPos, owner.ScaleData, summonEnemyData.MoveType, false, owner);

            if(childEnemy != null)
            {
                owner.ChildEnemy.Add(childEnemy);
            }
        }

        isExecuteOneTime = true;
    }
}
