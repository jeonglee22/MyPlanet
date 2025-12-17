using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SimpleShotPattern : ShootingPattern
{
    private float shootSpeed = 3f;
    public override int PatternId => (int)PatternIds.SimpleShot;

    public override bool RequireAsync => true;

    public SimpleShotPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
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

    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        int projectileCount = skillData.ProjectileQty;
        float projectileTerm = skillData.ProjectileTerm;

        for(int i = 0; i < projectileCount; i++)
        {
            token.ThrowIfCancellationRequested();

            Shoot(token);

            if(i < projectileCount - 1 && projectileTerm > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(projectileTerm), cancellationToken: token);
            }
        }
    }
}
