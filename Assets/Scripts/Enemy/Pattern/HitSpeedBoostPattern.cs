using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HitSpeedBoostPattern : SpecialPattern
{
    public override int PatternId => patternData.Pattern_Id;
    
    private float speedMultiplier = 3f;

    private GameObject changeAsset;
    private GameObject changedAsset;

    public HitSpeedBoostPattern()
    {
        Trigger = ExecutionTrigger.OnHit;
    }

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData data)
    {
        base.Initialize(enemy, movement, data);

        FindVisualAssets();

        SetInitialVisual();
    }

    public override void Execute()
    {
        if(isExecuteOneTime)
        {
            return;
        }

        BoostSpeed();

        ChangeVisual();

        isExecuteOneTime = true;
    }

    public override UniTask ExecuteAsync(CancellationToken token)
    {
        Execute();
        return UniTask.CompletedTask;
    }

    public override void PatternUpdate()
    {
        
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

    private void SetInitialVisual()
    {
        if(changeAsset != null && changedAsset != null)
        {
            changeAsset.SetActive(false);
            changedAsset.SetActive(true);
        }
    }

    private void BoostSpeed()
    {
        if(movement == null || owner == null)
        {
            return;
        }

        movement.ModifySpeed(speedMultiplier);
    }

    private void ChangeVisual()
    {
        if(changeAsset != null && changedAsset != null)
        {
            changeAsset.SetActive(true);
            changedAsset.SetActive(false);
        }
    }

    public override void Reset()
    {
        base.Reset();

        if(movement != null)
        {
            movement.ResetSpeed();
        }

        SetInitialVisual();
    }
}
