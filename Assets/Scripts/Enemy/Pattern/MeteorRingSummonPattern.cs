using UnityEngine;

public class MeteorRingSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    public MeteorRingSummonPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Summon()
    {
        Collider ownerCollider = owner.GetComponent<Collider>();
        float radius = 0f;
        if(ownerCollider != null)
        {
            var extents = ownerCollider.bounds.extents;
            radius = Mathf.Max(extents.x, extents.y, extents.z);
        }

        owner.Spawner.SpawnEnemiesInCircle(summonData.Enemy_Id, summonData.EnemyQuantity_1, radius, owner.ScaleData, false, owner.transform.position);
    }
}
