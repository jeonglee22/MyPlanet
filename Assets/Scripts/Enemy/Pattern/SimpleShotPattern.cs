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
        Vector3 spawnPos = owner.transform.position;
        Vector3 direction = (target.position - spawnPos).normalized;

        PatternProjectile projectile = spawner.SpawnPattern
        (
            patternData.Skill_Id,
            skillData.VisualAsset,
            spawnPos,
            direction,
            owner.atk,
            shootSpeed,
            skillData.Duration
        );
    }
}
