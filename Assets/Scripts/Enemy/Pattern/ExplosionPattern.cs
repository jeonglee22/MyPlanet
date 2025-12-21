using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExplosionPattern : SpecialPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private float explosionRadius = 3f;

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);
        Trigger = ExecutionTrigger.OnOrbitReached;

        RequireAsync = false;
    }

    public override void Execute()
    {
        if (isExecuteOneTime)
        {
            return;
        }

        isExecuteOneTime = true;

        DealExplosionDamage();
    
        owner.OnLifeTimeOver();
    }

    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        
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
}
