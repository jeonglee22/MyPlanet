using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UFOEnemy : Enemy
{
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
