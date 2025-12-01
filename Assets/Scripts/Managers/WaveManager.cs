using System;
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
    public int CurrentWaveIndex => currentWaveIndex;
    private bool isWaveInProgress = false;

    public bool IsWaveInProgress => isWaveInProgress;
    private int waveGroup;
    public int WaveCount {get; set;} = 1;

    private bool isCleared = false;
    public bool IsCleared => isCleared;

    private CancellationTokenSource waveCts;

    private float waveGroupDuration = 60f;
    private float waveGroupStartTime;
    private float pausedTime = 0f;
    private float pauseStartTime = 0f;
    private CancellationTokenSource groupCts;

    private bool isBossBattle = false;
    private bool isLastBoss = false;
    public bool IsBossBattle => isBossBattle;
    public bool IsLastBoss => isLastBoss;

    public event Action WaveChange;

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
        Variables.Reset();
    }

    private void OnDestroy()
    {
        Cancel();
        Variables.Reset();
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

    private void StartWaveGroupTimer()
    {
        waveGroupStartTime = Time.time;
        pausedTime = 0f;
        groupCts?.Cancel();
        groupCts?.Dispose();
        groupCts = new CancellationTokenSource();
    }

    private bool IsWaveGroupTimeExpired()
    {
        if(isBossBattle)
        {
            return false;
        }

        float elapsed = Time.time - waveGroupStartTime - pausedTime;
        return elapsed >= waveGroupDuration;
    }

    private async UniTask WaitForWaveGroupCompletion(CancellationToken cts)
    {
        float elapsed = Time.time - waveGroupStartTime - pausedTime;
        float remainingTime = waveGroupDuration - elapsed;

        if(remainingTime > 0)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(remainingTime), cancellationToken: cts);
        }
    }

    public async UniTask InitializeStage(int stageId)
    {
        Cancel();

        CancellationToken cts = waveCts.Token;

        currentStageId = stageId;

        waveDatas = DataTableManager.WaveTable.GetCurrentStageWaveData(stageId);

        if(waveDatas.Count == 0)
        {
            return;
        }

        waveGroup = waveDatas[0].WaveGroup;
        StartWaveGroupTimer();

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

        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts, groupCts.Token))
        {
            try
            {
                for(int i = 0; i < waveData.RepeatCount; i++)
                {
                    SpawnManager.Instance.SpawnCombination(combData, scaleData);
                    
                    if(i < waveData.RepeatCount - 1)
                    {
                        await UniTask.Delay(System.TimeSpan.FromSeconds(waveData.SpawnTerm), cancellationToken: linkedCts.Token);
                    }
                }

                await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: linkedCts.Token);
            }
            catch (System.OperationCanceledException)
            {
                
            }
        }

        await OnWaveCleared(cts);
    }

    public async UniTask StartNextWave(CancellationToken cts)
    {
        if(isWaveInProgress || currentWaveIndex >= waveDatas.Count)
        {
            return;
        }

        if(isBossBattle)
        {
            return;
        }

        if(currentWaveIndex > 0 && waveGroup != waveDatas[currentWaveIndex].WaveGroup)
        {
            if (!IsWaveGroupTimeExpired())
            {
                await WaitForWaveGroupCompletion(cts);
            }

            WaveCount++;
            waveGroup = waveDatas[currentWaveIndex].WaveGroup;
            WaveChange?.Invoke();
            StartWaveGroupTimer();
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

        if(IsWaveGroupTimeExpired())
        {
            while(currentWaveIndex < waveDatas.Count && waveGroup == waveDatas[currentWaveIndex].WaveGroup)
            {
                currentWaveIndex++;
            }
        }

        if(currentWaveIndex < waveDatas.Count && !isBossBattle)
        {
            await StartNextWave(cts);
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

        groupCts?.Cancel();
        groupCts?.Dispose();
        groupCts = new CancellationTokenSource();

        ResetWave();
    }

    public void OnBossSpawned(bool isLastBoss)
    {
        isBossBattle = true;
        this.isLastBoss = isLastBoss;

        pauseStartTime = Time.time;

        if(isLastBoss)
        {
            Debug.Log("Last Boss Spawned. Wave termination");
            groupCts?.Cancel();
            groupCts?.Dispose();
            groupCts = new CancellationTokenSource();
        }
        else
        {
            Debug.Log("Middle Boss Spawned. Wave progression paused.");
        }
    }

    public void OnBossDefeated(bool wasLastBoss)
    {
        if(Variables.LastBossEnemy != null)
        {
            return;
        }

        pausedTime += Time.time - pauseStartTime;

        isBossBattle = false;

        if (wasLastBoss)
        {
            isLastBoss = false;
            isCleared = true;
        }
        else
        {
            isLastBoss = false;

            if(currentWaveIndex < waveDatas.Count)
            {
                StartNextWave(waveCts.Token).Forget();
            }
        }
    }
}
