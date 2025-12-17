using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ParabolicShotPattern : ShootingPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float horizontalSpeed = 0.8f;
    private float verticalSpeed = -15f;
    private float upwardAccel = 12f;

    private float spawnOffsetMin = 0.3f;
    private float spawnOffsetMax = 1f;

    public override bool RequireAsync => true;

    public ParabolicShotPattern()
    {
        Trigger = ExecutionTrigger.OnInterval;
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
                await UniTask.Delay(System.TimeSpan.FromSeconds(projectileTerm), cancellationToken: token);
            }
        }
    }

    protected override void Shoot(CancellationToken token = default)
    {
        Vector3 spawnPos = owner.transform.position;

        float spawnOffset = Random.Range(spawnOffsetMin, spawnOffsetMax);
        float offsetDirection = Random.value > 0.5f ? 1f : -1f;
        spawnPos.x += offsetDirection * spawnOffset;

        float horizontaldirection = -offsetDirection;

        Vector3 initialVelocity = new Vector3(horizontaldirection * horizontalSpeed, verticalSpeed, 0f);

        Vector3 acceleration = new Vector3(0f, upwardAccel, 0f);

        PatternProjectile projectile = spawner.SpawnAccelPattern
        (
            patternData.Skill_Id,
            skillData.VisualAsset,
            spawnPos,
            initialVelocity,
            acceleration,
            1f,
            skillData.Duration
        );
    }
}
