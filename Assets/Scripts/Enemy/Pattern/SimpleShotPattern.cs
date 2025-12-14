using System.Threading;
using UnityEngine;

public class SimpleShotPattern : ShootingPattern
{
    private float shootSpeed = 3f;
    public override int PatternId => (int)PatternIds.SimpleShot;

    public SimpleShotPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
        TriggerValue = 2f;
    }

    protected override void Shoot(CancellationToken token = default)
    {
        Vector3 spawnPosition = owner.transform.position;
        Vector3 shootDirection = (target.position - owner.transform.position).normalized;

        spawner.SpawnPattern(spawnPosition, shootDirection, owner.atk, shootSpeed, owner.LifeTime);
    }
}
