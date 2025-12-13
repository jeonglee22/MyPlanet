using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
    public int enemyType = -1;

    public int spawnPoint = -1;

    public int enemyCount = 1;

    public int waveId = -1;
    public int moveTypeId = -1;

    public int patternId = -1;

    [SerializeField] private int[] enemyIds;
    [SerializeField] private int[] moveTypeIds;
    [SerializeField] private int[] waveIds;
    [SerializeField] private int[] patternIds;
    [SerializeField] private EnemySpawnTest enemySpawnTest;
    private int currentEnemyTypeId = -1;
    private EnemyTableData enemyData;
    private EnemyTableData choosedEnemyData;
    [SerializeField] private DpsCalculator dpsCalculator;

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
        SetEnemyStat();
        enemyData = MakeEnemyStatData();
    }

    public void EnemySpawn()
    {
        if (enemyTypeId == -1)
        {
            // spawn Wave Part
            if (waveId != -1)
            {
                
            }
        }
        else
        {
            if (spawnPoint < 0 || spawnPoint >= SpawnManager.Instance.Spawners.Count)
                return;

            if (moveTypeId < 0 || moveTypeId >= moveTypeIds.Length)
                return;

            var spawner = SpawnManager.Instance.Spawners[spawnPoint];

            // spawn Single Enemy
            spawner.SpawnEnemiesWithScale(enemyIds[enemyTypeId], enemyCount, new ScaleData()
            {
                HpScale = hpScale,
                AttScale = attackScale,
                DefScale = defenseScale,
                MoveSpeedScale = speedScale,
                PenetScale = penetrationScale,
                PrefabScale = sizeScale,
                ExpScale = expScale
            }, enemyData);
        }
    }

    private EnemyTableData MakeEnemyStatData()
    {
        if (enemyTypeId < 0)
            return null;

        if (moveTypeId < 0 || moveTypeId >= moveTypeIds.Length)
            return null;

        if  (patternId < 0 || patternId >= patternIds.Length)
            return null;

        var enemyData = new EnemyTableData
        {
            Enemy_Id = enemyIds[enemyTypeId],
            Hp = health,
            Attack = attack,
            Defense = defense,
            MoveSpeed = speed,
            UniqueRatePenetration = ratePenetration,
            FixedPenetration = fixedPenetration,
            Exp = exp,
            MoveType = moveTypeIds[moveTypeId],
            EnemyType = enemyType,
            PatternList = patternIds[patternId],
        };

        return enemyData;
    }

    protected virtual void SetEnemyStat()
    {
        if (currentEnemyTypeId == enemyTypeId || enemyTypeId == -1)
            return;

        var enemyData = DataTableManager.EnemyTable.Get(enemyIds[enemyTypeId]);
        if (enemyData == null)
            return;

        health = enemyData.Hp;
        attack = enemyData.Attack;
        defense = enemyData.Defense;
        barrior = enemyData.Shield;
        speed = enemyData.MoveSpeed;
        ratePenetration = enemyData.UniqueRatePenetration;
        fixedPenetration = enemyData.FixedPenetration;
        exp = (int)enemyData.Exp;
        enemyType = enemyData.EnemyType;
        currentEnemyTypeId = enemyTypeId;

        patternId = Array.IndexOf(patternIds, enemyData.PatternList);
        moveTypeId = Array.IndexOf(moveTypeIds, enemyData.MoveType);

        choosedEnemyData = enemyData;
    }

    public void ClearAllEnemies()
    {
        enemySpawnTest.ClearAllEnemies();
        dpsCalculator.ResetDps();
    }

    public void SetEnemyData()
    {
        if (enemyData == null)
            return;

        choosedEnemyData.Hp = health;
        choosedEnemyData.Attack = attack;
        choosedEnemyData.Defense = defense;
        choosedEnemyData.MoveSpeed = speed;
        choosedEnemyData.UniqueRatePenetration = ratePenetration;
        choosedEnemyData.FixedPenetration = fixedPenetration;
        choosedEnemyData.Exp = exp;
        choosedEnemyData.PatternList = patternIds[patternId];
        choosedEnemyData.MoveType = moveTypeIds[moveTypeId];
        choosedEnemyData.Shield = barrior;
        choosedEnemyData.EnemyType = enemyType;

        DataTableManager.EnemyTable.Set(enemyIds[enemyTypeId], choosedEnemyData);
    }
}