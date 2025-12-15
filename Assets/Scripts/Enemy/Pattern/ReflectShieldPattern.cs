using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class ReflectShieldPattern : SpecialPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private GameObject shieldObject;
    private ReflectShield reflectShield;

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);

        if(enemyData.EnemyType == 2)
        {
            Trigger = ExecutionTrigger.Immediate;
        }
        else if(enemyData.EnemyType == 3)
        {
            Trigger = ExecutionTrigger.ChildrenAlive;
        }

        RequireAsync = false;

        if(owner.ReflectShieldObject != null)
        {
            shieldObject = owner.ReflectShieldObject;
            reflectShield = shieldObject.GetComponent<ReflectShield>();

            shieldObject.SetActive(false);

            reflectShield.Initialize(owner);
        }
    }

    public override void Execute()
    {
        if(shieldObject != null)
        {
            shieldObject.SetActive(true);
            owner.IsReflectShieldActive = true;

            if(Trigger == ExecutionTrigger.Immediate)
            {
                isExecuteOneTime = true;
            }
        }
    }

    public override UniTask ExecuteAsync(CancellationToken token)
    {
        throw new System.NotImplementedException();
    }

    public override void PatternUpdate()
    {
        if(!owner.IsReflectShieldActive)
        {
            return;
        }

        if(Trigger == ExecutionTrigger.Immediate)
        {
            return;
        }

        if(Trigger == ExecutionTrigger.ChildrenAlive)
        {
            bool anyChildAlive = false;
            if(owner.ChildEnemy != null)
            {
                foreach(var child in owner.ChildEnemy)
                {
                    if(child != null && !child.IsDead)
                    {
                        anyChildAlive = true;
                        break;
                    }
                }
            }

            if(!anyChildAlive)
            {
                DeactiveateShield();
            }
        }
    }

    private void DeactiveateShield()
    {
        if(shieldObject != null)
        {
            shieldObject.SetActive(false);
        }

        owner.IsReflectShieldActive = false;
        isExecuteOneTime = false;
    }

    public override void Reset()
    {
        base.Reset();
        DeactiveateShield();
    }
}
