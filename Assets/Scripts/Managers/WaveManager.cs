using System.Collections.Generic;
using System.Threading;
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

    public bool IsWaveInProgress => isWaveInProgress;
    private int waveGroup;
    public int WaveCount {get; set;} = 1;

    private bool isCleared = false;
    public bool IsCleared => isCleared;

    private CancellationTokenSource waveCts;

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

        Cancel();
        ResetWave();
    }

    private void OnDestroy()
    {
        waveCts?.Cancel();
        waveCts?.Dispose();
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

        return enemyIds;
    }

    private async UniTask PreloadWaveAssets(WaveData waveData, CancellationToken cts)
    {
        var combData = DataTableManager.CombineTable.Get(waveData.Comb_Id);
        if(combData == null)
        {
            return;
        }

        var enemyIds = ExtractEnemyIds(combData);

        await GameManager.LoadManagerInstance.LoadEnemyPrefabsAsync(enemyIds);
        cts.ThrowIfCancellationRequested();

        foreach(int enemyId in enemyIds)
        {
            SpawnManager.Instance.PrepareEnemyPools(enemyId);
        }

        Debug.Log($"Preloaded assets for Wave ID: {waveData.Wave_Id}");
    }

    public async UniTask InitializeStage(int stageId)
    {
        CancellationToken cts = waveCts.Token;

        currentStageId = stageId;
        currentWaveIndex = 0;
        isWaveInProgress = false;

        waveDatas.Clear();
        waveDatas = DataTableManager.WaveTable.GetCurrentStageWaveData(stageId);

        if(waveDatas.Count == 0)
        {
            return;
        }

        waveGroup = waveDatas[0].WaveGroup;

        await PreloadWaveAssets(waveDatas[0], cts);
        await StartNextWave(cts);
    }

    private async UniTask ExecuteWave(WaveData waveData, CancellationToken cts)
    {
        Debug.Log($"Starting Wave ID: {waveData.Wave_Id}");
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
            //SpawnManager.Instance.SpawnCombination(combData, scaleData);
            
            if(i < waveData.RepeatCount - 1)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(waveData.SpawnTerm));
            }
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(waveData.SpawnTerm), cancellationToken: cts);

        await OnWaveCleared(cts);
    }

    public async UniTask StartNextWave(CancellationToken cts)
    {
        if(isWaveInProgress || currentWaveIndex >= waveDatas.Count)
        {
            return;
        }
        
        isWaveInProgress = true;
        var currentWave = waveDatas[currentWaveIndex];

        if(currentWaveIndex + 1 < waveDatas.Count)
        {
            await PreloadWaveAssets(waveDatas[currentWaveIndex + 1], cts);
        }

        await ExecuteWave(currentWave, cts);
    }

    public async UniTask OnWaveCleared(CancellationToken cts)
    {
        if(!isWaveInProgress)
        {
            return;
        }

        isWaveInProgress = false;
        currentWaveIndex++;

        if(currentWaveIndex < waveDatas.Count)
        {
            if(waveGroup != waveDatas[currentWaveIndex].WaveGroup)
            {
                WaveCount++;
                waveGroup = waveDatas[currentWaveIndex].WaveGroup;
            }
            await StartNextWave(cts);
        }
        else
        {
            isCleared = true;
            Debug.Log($"Stage {currentStageId} completed!");
        }
    }

    public void ResetWave()
    {
        currentWaveIndex = 0;
        isWaveInProgress = false;
        waveDatas.Clear();
    }

    public void Cancel()
    {
        waveCts?.Cancel();
        waveCts?.Dispose();
        waveCts = new CancellationTokenSource();

        ResetWave();
    }
}
