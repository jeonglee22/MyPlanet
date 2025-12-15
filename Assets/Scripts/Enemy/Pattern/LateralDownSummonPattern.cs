using UnityEngine;

public class LateralDownSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float horizontalOffset = 3f;

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

        Vector3 leftPosition = owner.transform.position + Vector3.left * horizontalOffset;
        Vector3 rightPosition = owner.transform.position + Vector3.right * horizontalOffset;

        int moveType = (int)MoveType.TwoPhaseDownMovement;

        owner.Spawner.SpawnEnemyAsChild(summonData.Enemy_Id, leftPosition, owner.ScaleData, moveType, false, owner);
        owner.Spawner.SpawnEnemyAsChild(summonData.Enemy_Id, rightPosition, owner.ScaleData, moveType, false, owner);
    }
}
