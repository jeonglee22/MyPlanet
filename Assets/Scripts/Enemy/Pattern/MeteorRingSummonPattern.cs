using UnityEngine;

public class MeteorRingSummonPattern : SummonPattern
{
    public override int PatternId => patternData.Pattern_Id;

    public MeteorRingSummonPattern()
    {
        Trigger = ExecutionTrigger.OnHealthPercentage;
    }

    protected override void Summon()
    {
        SphereCollider ownerCollider = owner.GetComponent<SphereCollider>();
        float radius = 0f;
        if(ownerCollider != null)
        {
            radius = ownerCollider.radius * owner.ScaleData.PrefabScale;
        }

        owner.Spawner.SpawnEnemiesInCircle(summonData.Enemy_Id, summonData.EnemyQuantity_1, radius, owner.ScaleData, false, owner.transform.position);
    }
}
