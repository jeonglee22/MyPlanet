using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public struct ScaleData
{
    public float HpScale;
    public float AttScale;
    public float DefScale;
    public float PenetScale;
    public float MoveSpeedScale;
    public float PrefabScale;
    public float ExpScale;
}

public class WaveManager : MonoBehaviour
{
    private static WaveManager instance;
    public static WaveManager Instance => instance;

    private int currentStageId;
    private List<WaveData> waveDatas = new List<WaveData>();
    private int currentWaveIndex = 0;
    private bool isWaveInProgress = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public HashSet<int> ExtractEnemyIds(CombineData combData)
    {
        HashSet<int> enemyIds = new HashSet<int>();

        if (combData.Enemy_Id_1 != 0)
            enemyIds.Add(combData.Enemy_Id_1);
        if (combData.Enemy_Id_2 != 0)
            enemyIds.Add(combData.Enemy_Id_2);
        if (combData.Enemy_Id_3 != 0)
            enemyIds.Add(combData.Enemy_Id_3);
        if (combData.Enemy_Id_4 != 0)
            enemyIds.Add(combData.Enemy_Id_4);
        if (combData.Enemy_Id_5 != 0)
            enemyIds.Add(combData.Enemy_Id_5);

        return enemyIds;
    }

    private async UniTask PreloadWaveAssets(WaveData waveData)
    {
        var combData = DataTableManager.CombineTable.Get(waveData.Comb_Id);
        if(combData == null)
        {
            return;
        }

        var enemyIds = ExtractEnemyIds(combData);

        await GameManager.LoadManagerInstance.LoadEnemyPrefabsAsync(enemyIds);

        foreach(int enemyId in enemyIds)
        {
            SpawnManager.Instance.PrepareEnemyPools(enemyId);
        }

        Debug.Log($"Preloaded assets for Wave ID: {waveData.Wave_Id}");
    }

    public async UniTask InitializeStage(int stageId)
    {
        currentStageId = stageId;
        currentWaveIndex = 0;
        isWaveInProgress = false;

        waveDatas.Clear();
        waveDatas = DataTableManager.WaveTable.GetCurrentStageWaveData(stageId);

        if(waveDatas.Count == 0)
        {
            return;
        }

        await PreloadWaveAssets(waveDatas[0]);

        await ExecuteWave(waveDatas[0]);
    }

    private async UniTask ExecuteWave(WaveData waveData)
    {
        var combData = DataTableManager.CombineTable.Get(waveData.Comb_Id);
        if(combData == null)
        {
            return;
        }

        ScaleData scaleData = new ScaleData
        {
            HpScale = waveData.HpScale,
            AttScale = waveData.AttScale,
            DefScale = waveData.DefScale,
            PenetScale = waveData.PenetScale,
            MoveSpeedScale = waveData.MoveSpeedScale,
            PrefabScale = waveData.PrefabScale,
            ExpScale = waveData.ExpScale
        };

        for(int i = 0; i < waveData.RepeatCount; i++)
        {
            SpawnManager.Instance.SpawnCombination(combData, scaleData);
            
            if(i < waveData.RepeatCount - 1)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(waveData.SpawnTerm));
            }
        }
    }
}
