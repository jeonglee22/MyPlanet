using UnityEngine;

public class LateralHommingSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float horizontalOffset = 3f;

    public LateralHommingSummonPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Summon()
    {
        if(owner?.Spawner == null)
        {
            return;
        }

        Vector3 leftPosition = owner.transform.position + Vector3.left * horizontalOffset;
        Vector3 rightPosition = owner.transform.position + Vector3.right * horizontalOffset;

        int moveType = (int)MoveType.TwoPhaseHomingMovement;

        owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, leftPosition, owner);
        owner.Spawner.SpawnEnemiesWithSummon(summonData.Enemy_Id, 1, owner.ScaleData, moveType, false, rightPosition, owner);
    }
}
