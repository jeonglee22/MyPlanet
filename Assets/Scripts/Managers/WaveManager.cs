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
    private bool isMiddleBoss = false;
    public bool IsBossBattle => isBossBattle;
    public bool IsLastBoss => isLastBoss;
    public bool IsMiddleBoss => isMiddleBoss;

    private int accumulateGold = 0;
    public int AccumulateGold => accumulateGold;

    public event Action WaveChange;
    public event Action LastBossSpawned;
    public event Action MiddleBossDefeated;
    public event Action OnGoldAccumulated;

    [SerializeField] private BattleUI battleUI;
    [SerializeField] private BossAppearEffect bossAppearEffect;

    private bool isTutorial = false;

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
        BattleRandomAbilityUnlockPatcher.ResetForNewBattle();
        Cancel();
        ResetWave();
        Variables.Reset();
        accumulateGold = 0;
    }

    private void Start()
    {
        SetIsTutorial(TutorialManager.Instance?.IsTutorialMode ?? false);
    }

    private void OnDestroy()
    {
        if(accumulateGold > 0)
        {
            SaveAccumulatedGold().Forget();
        }

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

        if (waveData.RepeatCount == 0 && waveData.SpawnTerm == 0f)
        {
            bossAppearEffect.gameObject.SetActive(true);
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
        if (Variables.IsTestMode)
        {
            return;
        }

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
        await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: cts);
        await StartNextWave(cts);
    }

    public async UniTask ExecuteWave(WaveData waveData, CancellationToken cts)
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
                if(waveData.RepeatCount == 0)
                {
                    SpawnManager.Instance.SpawnCombination(combData, scaleData);
                    
                    await UniTask.Delay(System.TimeSpan.FromSeconds(waveData.SpawnTerm), cancellationToken: linkedCts.Token);
                }
                else
                {
                    for(int i = 0; i < waveData.RepeatCount; i++)
                    {
                        if(IsWaveGroupTimeExpired())
                        {
                            break;
                        }

                        SpawnManager.Instance.SpawnCombination(combData, scaleData);
                        
                        await UniTask.Delay(System.TimeSpan.FromSeconds(waveData.SpawnTerm), cancellationToken: linkedCts.Token);
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                
            }
        }

        if (Variables.IsTestMode)
        {
            ResetWave();
            return;
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

        var clearedWave = waveDatas[currentWaveIndex];
        if(clearedWave.WaveRewardGold > 0)
        {
            accumulateGold += clearedWave.WaveRewardGold;
            OnGoldAccumulated?.Invoke();
            battleUI.AddCoinGainText(clearedWave.WaveRewardGold);
        }

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
            // var currentWave = waveDatas[currentWaveIndex];
            // var waveReward = currentWave.WaveReward;
            // if (waveReward > 0)
            // {
            //     battleUI.AddCoinGainText(waveReward);   
            // }
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
        if (isLastBoss)
        {
            this.isLastBoss = true;
        }
        else
        {
            isMiddleBoss = true;

            if (this.isLastBoss)
            {
                LastBossSpawned?.Invoke();
            }
        }

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
            MiddleBossDefeated?.Invoke();
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
            if(currentWaveIndex < waveDatas.Count)
            {
                StartNextWave(waveCts.Token).Forget();
            }
        }
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }

    public async UniTask SaveAccumulatedGold()
    {
        if(accumulateGold <= 0)
        {
            return;
        }

        await UniTask.WaitUntil(() => CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized);

        int currentGold = CurrencyManager.Instance.CachedGold;
        int newGold = currentGold + accumulateGold;

        CurrencyManager.Instance.SetGold(newGold);
        await CurrencyManager.Instance.SaveCurrencyAsync();

        accumulateGold = 0;
    }
}