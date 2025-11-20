using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class MeteorClusterPattern : MovementPattern
{
    private PatternSpawner spawner;

    private int meteorCount;
    private bool isSpawning;
    private bool hasCollided;
    private float accumulateDamage = 0f;
    private float damageThreshold;

    private List<PatternProjectile> patterns = new List<PatternProjectile>();

    private float spawnRadius = 1f;

    private void OnDisable()
    {
        returnAllPatterns();

        if(owner != null)
        {
            owner.OnDeathEvent -= OnOwnerDeath;
        }
    }

    private void OnDestroy()
    {
        returnAllPatterns();

        if(owner != null)
        {
            owner.OnDeathEvent -= OnOwnerDeath;
        }
    }

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData, ExecutionTrigger trigger = ExecutionTrigger.None, float interval = 0f)
    {
        base.Initialize(enemy, movement, enemyData, trigger, interval);

        originalSpeed = movement.moveSpeed;
        patterns.Clear();
        isSpawning = false;
        hasCollided = false;

        spawner = PatternSpawner.Instance;

        owner.OnDeathEvent += OnOwnerDeath;

        Cancel();
        SpawnSequence(cts.Token).Forget();
    }

    private async UniTaskVoid SpawnSequence(CancellationToken token)
    {
        try
        {
            isSpawning = true;

            movement.moveSpeed = 0f;
            owner.PauseLifeTime();

            meteorCount = UnityEngine.Random.Range(3, 6);

            damageThreshold = owner.maxHp / meteorCount;

            for(int i = 0; i < meteorCount; i++)
            {
                if(token.IsCancellationRequested)
                {
                    break;
                }

                SpawnPatterns();

                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
            }

            if(!token.IsCancellationRequested)
            {
                isSpawning = false;

                movement.moveSpeed = originalSpeed;
                owner.ResumeLifeTime();

                foreach(var pattern in patterns)
                {
                    pattern.SetCanMove(true);
                }
            }
        }
        catch(OperationCanceledException)
        {
            // Ignore cancellation
        }
    }

    private void RemoveOnePattern()
    {
        if(patterns.Count == 0)
        {
            return;
        }

        PatternProjectile pattern = patterns[0];
        patterns.RemoveAt(0);
        pattern.ReturnToPool();

        if(patterns.Count == 0)
        {
            owner.OnDamage(owner.maxHp);
        }
    }

    private void OnPatternHitByProjectile(PatternProjectile pattern, float projectileDamage)
    {
        accumulateDamage += projectileDamage;

        if(accumulateDamage >= damageThreshold)
        {
            accumulateDamage -= damageThreshold;
            RemoveOnePattern();
        }
    }

    private void OnPatternHitPlayer(PatternProjectile pattern)
    {
        if (hasCollided)
        {
            return;
        }
        
        hasCollided = true;

        float totalDamage = owner.atk * patterns.Count;
        pattern.SetDamage(totalDamage);

        foreach(var p in patterns)
        {
            if(p != pattern)
            {
                p.SetCanDealDamage(false);
            }
        }
    }

    private void SpawnPatterns()
    {
        var pos = GetRandomPositionInCircle();
        float clusterLifeTime = owner.RemainingLifeTime;

        PatternProjectile pattern = spawner.SpawnPattern(pos, owner.Movement.MoveDirection, owner.atk, originalSpeed, clusterLifeTime);

        if(pattern != null)
        {
            pattern.SetCanMove(false);

            pattern.OnHitByProjectileEvent += OnPatternHitByProjectile;
            pattern.OnPlayerHitEvent += OnPatternHitPlayer;

            patterns.Add(pattern);
        }
    }

    public override float CalculateDamage(float damage)
    {
        if(isSpawning || patterns.Count > 0)
        {
            return 0f;
        }

        return damage;
    }

    private Vector3 GetRandomPositionInCircle()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
        return owner.transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
    }

    protected override void ChangeMovement()
    {
        if(movement != null)
        {
            Destroy(movement);
        }

        var homingMovement = owner.gameObject.AddComponent<HomingMovement>();
        homingMovement.Initialize(originalSpeed, Vector3.zero);

        Transform playerTransform = GameObject.FindGameObjectWithTag(TagName.Planet).transform;
        if(playerTransform != null)
        {
            foreach(var pattern in patterns)
            {
                if(pattern != null)
                {
                    Vector3 direction = (playerTransform.position - pattern.transform.position).normalized;
                    pattern.MoveDirection = direction;
                }
            }
        }
    }

    private void returnAllPatterns()
    {
        foreach(var pattern in patterns)
        {
            if(pattern != null)
            {
                pattern.ReturnToPool();
            }
        }

        patterns.Clear();
    }

    private void OnOwnerDeath()
    {
        returnAllPatterns();
    }
}
