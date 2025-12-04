using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LazerPattern : ShootingPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private List<GameObject> lazerPool = new List<GameObject>();

    protected float duration = 2f;
    protected float laserWidth = 0.5f;
    protected float tickInterval = 0.1f;

    public override bool RequireAsync => true;

    protected UniTaskCompletionSource lazerCompletionSource;


    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);

        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Shoot()
    {
        Vector3 shootPosition = owner.transform.position;
        Vector3 shootDirection = GetLaserDirection();

        GameObject lazerObject = GetOrCreateLazer();
        Lazer lazer = lazerObject.GetComponent<Lazer>();

        lazer.SetDuration(duration);
        lazer.SetLazerWidth(laserWidth);
        lazer.SetTickInterval(tickInterval);

        float damage = patternData.PatternDamageRate > 0 ? owner.atk * patternData.PatternDamageRate : owner.atk;

        if(movement != null)
        {
            movement.CanMove = false;
        }

        lazer.Initialize(shootPosition, shootDirection, damage, OnLazerComplete);

        lazerObject.SetActive(true);
    }

    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        lazerCompletionSource = new UniTaskCompletionSource();

        Shoot();

        await lazerCompletionSource.Task.AttachExternalCancellation(token);
    }

    protected virtual Vector3 GetLaserDirection()
    {
        return (target.position - owner.transform.position).normalized;
    }

    private void OnLazerComplete()
    {
        if(movement != null)
        {
            movement.CanMove = true;
        }

        planet.IsLazerHit = false;

        lazerCompletionSource?.TrySetResult();
    }

    protected GameObject GetOrCreateLazer()
    {
        foreach(var lazer in lazerPool)
        {
            if(!lazer.activeInHierarchy)
            {
                return lazer;
            }
        }

        GameObject newLazer = LoadManager.GetLoadedGamePrefab(AddressLabel.EnemyLazer);
        lazerPool.Add(newLazer);
        return newLazer;
    }
}
