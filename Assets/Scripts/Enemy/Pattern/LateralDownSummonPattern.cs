using UnityEngine;

public class LateralDownSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float horizontalOffset = 3f;

    private bool isRight = false;

    public LateralDownSummonPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Summon()
    {
        if(owner?.Spawner == null)
        {
            return;
        }

        Vector3[] spawnPosition = {owner.transform.position + Vector3.left * horizontalOffset, owner.transform.position + Vector3.right * horizontalOffset};

        int moveType = (int)MoveType.TwoPhaseDownMovement;

        if(PatternIds.NeptuneFrontDiaSummon == (PatternIds)PatternId)
        {
            owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, spawnPosition[0], owner);
            owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, spawnPosition[1], owner);
        }
        else if(PatternIds.NeptuneBigDiaSummon == (PatternIds)PatternId)
        {
            if (isRight)
            {
                owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, spawnPosition[1], owner);
                isRight = false;
            }
            else
            {
                owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, spawnPosition[0], owner);
                isRight = true;
            }
        }
    }
}
