using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExplosionPattern : SpecialPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float explosionRadius = 3f;

    private GameObject changeAsset;
    private GameObject changedAsset;
    private ParticleSystem explosionEffect;

    private Collider enemyCollider;
    private Canvas statusCanvas;

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);
        Trigger = ExecutionTrigger.OnOrbitReached;

        RequireAsync = true;

        FindVisualAssets();

        FindCompoenents();

        SetupExplosionEffect();

        SetInitialVisual();
    }

    public override void Execute()
    {
        
    }

    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        if (isExecuteOneTime)
        {
            return;
        }

        isExecuteOneTime = true;

        if(movement != null)
        {
            movement.CanMove = false;
        }

        if(enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if(statusCanvas != null)
        {
            statusCanvas.gameObject.SetActive(false);
        }

        DealExplosionDamage();

        ChangeToExplosionVisual();

        if(explosionEffect != null)
        {
            explosionEffect.Play();
            
            float duration = explosionEffect.main.duration + explosionEffect.main.startLifetime.constantMax;
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
        }

        ResetState();
    
        owner.OnLifeTimeOver();
    }

    public override void PatternUpdate()
    {
        
    }

    private void DealExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(owner.transform.position, explosionRadius, LayerMask.GetMask(TagName.Planet));

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(TagName.Planet))
            {
                var damagable = collider.GetComponent<IDamagable>();
                float damage = patternData.PatternDamageRate > 0 ? owner.atk * patternData.PatternDamageRate : owner.atk;
                damagable?.OnDamage(damage);
            }
        }
    }

    private void FindVisualAssets()
    {
        if(owner == null)
        {
            return;
        }

        Transform[] children = owner.GetComponentsInChildren<Transform>(true);

        foreach(Transform child in children)
        {
            if(child.CompareTag(TagName.ChangeVisual))
            {
                changeAsset = child.gameObject;
                explosionEffect = changeAsset.GetComponent<ParticleSystem>();
            }
            else if(child.CompareTag(TagName.ChangedVisual))
            {
                changedAsset = child.gameObject;
            }

            if(changeAsset != null && changedAsset != null)
            {
                break;
            }
        }
    }

    private void SetupExplosionEffect()
    {
        if(explosionEffect != null)
        {
            var shape = explosionEffect.shape;
            shape.radius = explosionRadius;
        }
    }

    private void SetInitialVisual()
    {
        if(changeAsset != null && changedAsset != null)
        {
            changeAsset.SetActive(false);
            changedAsset.SetActive(true);
        }
    }

    private void ChangeToExplosionVisual()
    {
        if(changeAsset != null && changedAsset != null)
        {
            changeAsset.SetActive(true);
            changedAsset.SetActive(false);
        }
    }

    private void FindCompoenents()
    {
        if(owner == null)
        {
            return;
        }

        enemyCollider = owner.GetComponent<SphereCollider>();

        statusCanvas = owner.GetComponentInChildren<Canvas>(true);
    }

    private void ResetState()
    {
        if(movement != null)
        {
            movement.CanMove = true;
        }

        if(enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        if(statusCanvas != null)
        {
            statusCanvas.gameObject.SetActive(true);
        }

        SetInitialVisual();
    }   
}
