using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SkillBasedLazerPattern : ShootingPattern
{
    public override int PatternId => patternData.Pattern_Id;
    public override bool RequireAsync => true;

    private List<GameObject> lazerPool = new List<GameObject>();
    protected UniTaskCompletionSource lazerCompletionSource;

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);
        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Shoot(CancellationToken token = default)
    {
        Vector3 shootPosition = owner.transform.position;

        GameObject lazerObject = GetOrCreateLazer();
        SkillBasedLazer lazer = lazerObject.GetComponent<SkillBasedLazer>();

        float skillDamageRate = skillData.SkillDamage > 0 ? skillData.SkillDamage : 1f;
        float damage = patternData.PatternDamageRate > 0 ? owner.atk * patternData.PatternDamageRate * skillDamageRate : owner.atk * skillDamageRate;

        if(movement != null)
        {
            movement.CanMove = false;
        }

        Transform targetTransform = GetLazerTarget();
        lazer.Initialize(shootPosition, targetTransform, damage, skillData, owner.transform, OnLazerComplete, token);

        lazerObject.SetActive(true);
    }

    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        lazerCompletionSource = new UniTaskCompletionSource();

        Shoot(token);

        await lazerCompletionSource.Task.AttachExternalCancellation(token);
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

        GameObject newLazer = LoadManager.GetLoadedGamePrefab(skillData.VisualAsset);
        lazerPool.Add(newLazer);
        return newLazer;
    }

    protected virtual Transform GetLazerTarget()
    {
        return null;
    }
}
