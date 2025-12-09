using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyStatTester : MonoBehaviour
{
    public int enemyTypeId = -1;
    public float hpScale = 1f;
    public float attackScale = 1f;
    public float defenseScale = 1f;
    public float penetrationScale = 1f;
    public float speedScale = 1f;
    public float sizeScale = 1f;
    public float expScale = 1f;

    public float health = 100f;
    public float defense = 0f;
    public float barrior = 0f;
    public float speed = 1f;
    public float attack = 1f;
    public float ratePenetration = 0f;
    public float fixedPenetration = 0f;
    public int exp = 10;

    public int enemyCount = 1;

    public int waveId = -1;
    public int moveTypeId = -1;

    [SerializeField] private int[] enemyIds;
    [SerializeField] private int[] moveTypeIds;
    [SerializeField] private int[] waveIds;
    public int[] WaveIds => waveIds;

    private async UniTaskVoid Start()
    {
        Initialize();

        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);
    }

    private void Initialize()
    {
    }

    void FixedUpdate()
    {
        TestBalance();
        MakeEnemyStatData();
    }

    private EnemyTableData MakeEnemyStatData()
    {
        if (enemyTypeId < 0)
            return null;

        var enemyData = new EnemyTableData
        {
            Enemy_Id = enemyTypeId,
            Hp = health * hpScale,
            Attack = attack * attackScale,
            Defense = defense * defenseScale,
            MoveSpeed = speed * speedScale,
            UniqueRatePenetration = ratePenetration * penetrationScale,
            FixedPenetration = fixedPenetration * penetrationScale,
            Exp = exp * expScale
        };

        return enemyData;
    }

    protected virtual void TestBalance()
    {
        
    }
}