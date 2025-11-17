using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MeteorClusterPattern : EnemyPattern
{
    private int meteorCount;
    private int spawnedCount = 0;
    private bool isSpawning;
    private Transform playerTransform;
    private float originalSpeed;

    private List<Enemy> childMeteors = new List<Enemy>();

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);
        owner = enemy;

        meteorCount = UnityEngine.Random.Range(4, 5);
        spawnedCount = 0;
        originalSpeed = movement.moveSpeed;
        childMeteors.Clear();

        isSpawning = true;
        playerTransform = GameObject.FindGameObjectWithTag("Planet").transform;
        movement.moveSpeed = 0f;

        owner.PauseLifeTime();

        Cancel();
        SpawnSequence(cts.Token).Forget();
    }

    private async UniTaskVoid SpawnSequence(CancellationToken token)
    {
        try
        {
            for(int i = 0; i < meteorCount; i++)
            {
                if(token.IsCancellationRequested)
                {
                    break;
                }

                SpawnChildMeteor();
                spawnedCount++;

                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
            }

            if(!token.IsCancellationRequested)
            {
                isSpawning = false;
                movement.moveSpeed = originalSpeed;

                owner.ResumeLifeTime();

                foreach(var meteor in childMeteors)
                {
                    meteor.Movement.moveSpeed = originalSpeed;
                    meteor.ResumeLifeTime();
                }
            }
        }
        catch(OperationCanceledException)
        {
            // Ignore cancellation
        }
    }

    private void SpawnChildMeteor()
    {
        Enemy child = owner.Spawner.SpawnEnemy(data.Enemy_Id, owner.transform.position, excutePattern: false);

        if(child != null)
        {
            child.Movement.moveSpeed = 0f;
            child.PauseLifeTime();
            childMeteors.Add(child);
        }
    }

    public override float CalculateDamage(float damage)
    {
        if(isSpawning)
        {
            return 0f;
        }

        meteorCount--;

        if (meteorCount > 0)
        {
            return 0;
        }
        else
        {
            return damage;
        }
    }

    public override void OnTrigger(Collider other)
    {
        if(other.CompareTag("PatternLine") && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - owner.transform.position).normalized;

            movement.SetDirection(direction);
        }
    }
}
