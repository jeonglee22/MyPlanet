using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MeteorClusterPattern : SpecialPattern
{
    private EnemySpawner spawner;
    public override int PatternId => patternData.Pattern_Id;

    private int meteorCount = 6;
    private float damage = 10f;
    private float spawnRadius = 0.5f;

    private Enemy currentLeader;
    private IMovement leaderMovement;

    private List<Enemy> meteorChildren = new List<Enemy>();
    private List<Vector3> childOffsets = new List<Vector3>();
    private List<IMovement> childMovements = new List<IMovement>();

    private int childMoveType = (int)MoveType.FollowParent;

    public MeteorClusterPattern()
    {
        Trigger = ExecutionTrigger.Immediate;
        TriggerValue = 0f;
    }

    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);

        damage = enemy.atk;
        spawner = enemy.Spawner;

        currentLeader = enemy;
        leaderMovement = movement.CurrentMovement;

        enemy.OnDeathEvent += OnLeaderDeath;
        enemy.OnLifeTimeOverEvent += OnLeaderDeath;
    }

    public override void Execute()
    {
        if(isExecuteOneTime || spawner == null)
        {
            return;
        }

        CreateChildren();
        isExecuteOneTime = true;
    }

    public override void PatternUpdate()
    {
        
    }

    private void CalculateChildOffsets()
    {
        childOffsets.Clear();

        float angleStep = 360f / meteorCount;

        for(int i = 0; i < meteorCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offSet = new Vector3(Mathf.Cos(angle) * spawnRadius, Mathf.Sin(angle) * spawnRadius, 0f);
            childOffsets.Add(offSet);
        }
    }

    private void CreateChildren()
    {
        int enemyId = enemyData.Enemy_Id;

        spawner.PreparePool(enemyId);

        CalculateChildOffsets();

        ScaleData scaleData = new ScaleData
        {
            HpScale = 1f,
            AttScale = 1f,
            DefScale = 1f,
            MoveSpeedScale = 1f,
            PenetScale = 1f,
            PrefabScale = 1f
        };

        for(int i = 0; i < childOffsets.Count; i++)
        {
            Vector3 spawnPosition = owner.transform.position + childOffsets[i];

            Enemy childMeteor = spawner.SpawnEnemyAsChild(enemyId, spawnPosition, scaleData, childMoveType, owner.ShouldDropItems);
            var followMovement = childMeteor.Movement.CurrentMovement as FollowParentMovement;

            if(childMeteor != null)
            {
                followMovement.SetParent(currentLeader.transform, childOffsets[i]);
                childMeteor.OnCollisionDamageCalculate = CalculateCollisionDamage;
                childMeteor.OnDeathEvent += () => OnChildDeath(childMeteor);
                childMeteor.OnLifeTimeOverEvent += () => OnChildDeath(childMeteor);
                meteorChildren.Add(childMeteor);
                childMovements.Add(followMovement);
            }
        }
    }

    public int GetAliveChildCount()
    {
        int count = 0;
        foreach(var child in meteorChildren)
        {
            if(child != null && !child.IsDead)
            {
                count++;
            }
        }

        return count;
    }

    public float CalculateCollisionDamage()
    {
        int aliveCount = GetAliveChildCount();
        if(currentLeader != null && !currentLeader.IsDead)
        {
            aliveCount++;
        }
        return damage * aliveCount;
    }

    public void OnChildDeath(Enemy child)
    {
        int childIndex = meteorChildren.IndexOf(child);
        bool wasLeader = (child == currentLeader);

        if(childIndex >= 0 && childIndex < meteorChildren.Count)
        {
            meteorChildren.RemoveAt(childIndex);

            if(childIndex < childMovements.Count)
            {
                childMovements.RemoveAt(childIndex);
            }
            if(childIndex < childOffsets.Count)
            {
                childOffsets.RemoveAt(childIndex);
            }
        }

        if(wasLeader)
        {
            PromoteNewLeader();
        }
    }

    private void ClearChildren()
    {
        foreach(var child in meteorChildren)
        {
            if(child != null && !child.IsDead)
            {
                child.Die();
            }
        }

        meteorChildren.Clear();
        childOffsets.Clear();
        childMovements.Clear();
    }

    public override void Reset()
    {
        base.Reset();

        if(currentLeader != null)
        {
            currentLeader.OnDeathEvent -= OnLeaderDeath;
            currentLeader.OnLifeTimeOverEvent -= OnLeaderDeath;
        }

        ClearChildren();
        currentLeader = null;
    }

    private void OnLeaderDeath()
    {
        PromoteNewLeader();
    }

    private void PromoteNewLeader()
    {
        Enemy newLeader = null;
        int newLeaderIndex = -1;

        for(int i = 0; i < meteorChildren.Count; i++)
        {
            Enemy child = meteorChildren[i];
            if(child != null && !child.IsDead && child.gameObject.activeSelf)
            {
                newLeader = child;
                newLeaderIndex = i;
                break;
            }
        }

        if(newLeader == null)
        {
            currentLeader = null;
            return;
        }

        if(currentLeader != null)
        {
            currentLeader.OnDeathEvent -= OnLeaderDeath;
            currentLeader.OnLifeTimeOverEvent -= OnLeaderDeath;
        }

        Enemy oldLeader = currentLeader;
        currentLeader = newLeader;

        Vector3 currentDirection = Vector3.down;
        if(oldLeader != null && oldLeader.Movement != null)
        {
            IMovement oldLeaderMovement = oldLeader.Movement.CurrentMovement;
            newLeader.Movement.CurrentMovement = oldLeaderMovement;
            newLeader.Movement.MoveDirection = oldLeaderMovement.GetFinalDirection(currentDirection, oldLeader.transform, null);

            if(oldLeaderMovement is ChaseMovement chaseMovement)
            {
                chaseMovement.OnPatternLine();
            }
        }
        
        /*
        if(newLeader.Movement != null)
        {
            IMovement leaderMovementCopy = CopyMovement(leaderMovement);
            float moveSpeed = oldLeader != null ? oldLeader.Data.MoveSpeed : owner.Data.MoveSpeed;
            newLeader.Movement.Initialize(moveSpeed, -1, leaderMovementCopy);

            if(leaderMovementCopy is ChaseMovement chaseMovement)
            {
                chaseMovement.OnPatternLine();
            }
        }
        */
        newLeader.OnDeathEvent += OnLeaderDeath;
        currentLeader.OnLifeTimeOverEvent += OnLeaderDeath;

        if(newLeaderIndex >= 0 && newLeaderIndex < childOffsets.Count)
        {
            childOffsets.RemoveAt(newLeaderIndex);
            childMovements.RemoveAt(newLeaderIndex);
            meteorChildren.RemoveAt(newLeaderIndex);
        }

        for(int i = 0; i < meteorChildren.Count; i++)
        {
            Enemy child = meteorChildren[i];
            if(child == null || child.IsDead || child == newLeader || !child.gameObject.activeSelf)
            {
                continue;
            }

            if(i < childMovements.Count)
            {
                FollowParentMovement followMovement = childMovements[i] as FollowParentMovement;
                if(followMovement != null && i < childOffsets.Count)
                {
                    Vector3 newOffset = child.transform.position - currentLeader.transform.position;
                    followMovement.SetParent(currentLeader.transform, newOffset);
                }
            }
        }
    }

    private IMovement CopyMovement(IMovement original)
    {
        if(original is StraightDownMovement)
        {
            return new StraightDownMovement();
        }
        else if(original is HomingMovement)
        {
            return new HomingMovement();
        }
        else if(original is ChaseMovement)
        {
            return new ChaseMovement();
        }
        else if(original is FollowParentMovement)
        {
            return new FollowParentMovement();
        }

        return null;
    }
}
