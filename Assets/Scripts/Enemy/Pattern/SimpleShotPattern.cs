using UnityEngine;

public class SimpleShotPattern : ShootingPattern
{
    private float shootSpeed = 3f;
    public override int PatternId => (int)PatternIds.SimpleShot;

    public SimpleShotPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
        TriggetValue = 2f;
    }

    protected override void Shoot()
    {
        Vector3 spawnPosition = owner.transform.position;
        Vector3 shootDirection = Vector3.down;

        spawner.SpawnPattern(spawnPosition, shootDirection, owner.atk, shootSpeed, owner.LifeTime);
    }
}
