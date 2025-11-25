using UnityEngine;

public class MeteorClusterSummonPattern : SummonPattern
{
    public override int PatternId => (int)PatternIds.MeteorClusterSummon;

    private int summonEnemyId;
    private IMovement summonMovement;
    private ScaleData scaleData;
    private ISpawnLocationProvide spawnLocationProvider;

    

    protected override void Summon()
    {
        throw new System.NotImplementedException();
    }
}
