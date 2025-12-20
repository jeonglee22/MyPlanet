using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UFOEnemy : Enemy
{
    private static UFOEnemy currentInstance = null;

    protected override void OnDisable()
    {
        if(currentInstance == this)
        {
            currentInstance = null;
        }

        base.OnDisable();
    }

    public override void Initialize(EnemyTableData enemyData, int enemyId, ObjectPoolManager<int, Enemy> poolManager, ScaleData scaleData, int spawnPointIndex)
    {
        if(currentInstance != null && currentInstance != this)
        {
            currentInstance.OnLifeTimeOver();
        }

        currentInstance = this;

        base.Initialize(enemyData, enemyId, poolManager, scaleData, spawnPointIndex);
    }

    protected override async UniTaskVoid LifeTimeTask(CancellationToken token)
    {
        try
        {
            await UniTask.CompletedTask;
        }
        catch (System.OperationCanceledException)
        {

        }
    }
}
