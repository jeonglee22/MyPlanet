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

    private bool isTutorial = false;

    [Header("BGM")]
    [SerializeField] private AudioClip battleBgm;
    [SerializeField, Range(0f, 1f)] private float battleBgmVolume = 1f;
    [SerializeField] private AudioSource bgmSource;
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

        EnsureBgmSource();
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
        StopBattleBgm();
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
        if (Variables.IsTestMode)
        {
            return;
        }

        Cancel();
        StartBattleBgm();

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
        StopBattleBgm();
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
            StopBattleBgm();
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

    private void EnsureBgmSource()
    {
        if (bgmSource != null) return;

        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f; 
    }

    private void StartBattleBgm()
    {
        if (battleBgm == null) return;

        EnsureBgmSource();

        if (bgmSource.isPlaying && bgmSource.clip == battleBgm) return;

        bgmSource.clip = battleBgm;
        bgmSource.volume = battleBgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private void StopBattleBgm()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
        bgmSource.clip = null;
    }
}
