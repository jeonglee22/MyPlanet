using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    private Dictionary<int, string> StageName = new Dictionary<int, string>()
    {
        {1, "1-1"},
        {2, "1-2"},
        {3, "1-3"},
        {4, "1-4"},
        {5, "1-5"},
        {6, "2-1"},
        {7, "2-2"},
        {8, "2-3"},
        {9, "2-4"},
        {10, "2-5"},
    };

    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToTitleButton;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TowerInstallControl installControl;

    [SerializeField] private TextMeshProUGUI gameResultText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI enemyKillText;

    [SerializeField] private Button checkButton;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Button detailButton;

    [SerializeField] private List<Image> rewardItemImages;
    [SerializeField] private List<TextMeshProUGUI> rewardItemTexts;
    [SerializeField] private Planet planet;

    private StageData currentStageData;
    private bool isGameClear = false;

    void Start()
    {
        WaveManager.Instance.Cancel();
        // restartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        restartButton?.onClick.AddListener(OnRestartCliecked);
        checkButton?.onClick.AddListener(OnReturnToTitleClicked);
        detailButton?.onClick.AddListener(OnOpenDetailPanelClicked);
    }

    private void OnOpenDetailPanelClicked()
    {
        detailPanel.SetActive(true);
        var detailUI = detailPanel.GetComponent<TowerDamageInfoUI>();
        detailUI.SetTowerDamageInfos(planet);
    }

    public void SetGameResultText(bool gameClear, int stageIndex, string playTime, int enemyKillCount)
    {
        isGameClear = gameClear;
        currentStageData = DataTableManager.StageTable.GetByIndex(stageIndex);

        if(gameResultText != null)
        {
            gameResultText.text = gameClear ? "승리" : "패배";
        }

        if(stageText != null)
        {
            stageText.text = $"스테이지 {StageName[stageIndex]}";
        }

        if(timeText != null)
        {
            timeText.text = playTime;
        }

        if(enemyKillText != null)
        {
            enemyKillText.text = $"{enemyKillCount}";
        }

        ProcessWaveGold().Forget();

        if (gameClear)
        {
            ProcessStageRewards().Forget();
        }
    }

    public void SetRewardItems(List<(int itemId, int itemCount)> dropItems)
    {
        

        restartButton?.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        returnToTitleButton?.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
    }

    public void SetResultText(bool isWin)
    {
        if(resultText == null) return;

        if (isWin)
        {
            resultText.text = "Stage Clear!!";
        }
        else
        {
            resultText.text = "Stage Fail!!";
        }
    }

    public void OnRestartCliecked()
    {
        UserTowerManager.Instance.UpdateUserTowerDataAsync(installControl).Forget();

        SceneControlManager.Instance.LoadScene(SceneControlManager.Instance.CurrentSceneName).Forget();
    }

    public void OnReturnToTitleClicked()
    {
        UserTowerManager.Instance.UpdateUserTowerDataAsync(installControl).Forget();

        SceneControlManager.Instance.LoadScene(SceneName.LobbyScene).Forget();
    }

    private async UniTask ProcessStageRewards()
    {
        await UniTask.WaitUntil(() => UserStageManager.Instance != null && UserPlanetManager.Instance.IsInitialized && UserStageManager.Instance.IsInitialized);
        
        if(currentStageData == null)
        {
            return;
        }

        int highestCleared = UserStageManager.Instance.ClearedStageData.HighestClearedStage;
        bool isFirstClear = Variables.Stage > highestCleared;

        int rewardId = isFirstClear ? currentStageData.FirstReward_Id : currentStageData.Reward_Id;

        var rewardData = DataTableManager.StageRewardTable.Get(rewardId);
        if(rewardData == null)
        {
            return;
        }

        PrecessRewards(rewardData);

        await SaveAllDataToFirebase();

        if (isFirstClear)
        {
            await UserStageManager.Instance.SaveUserStageClearAsync(Variables.Stage);
        }
    }

    private void PrecessRewards(StageRewardData rewardData)
    {
        if(rewardData.Target_Id_1 != 0 && rewardData.RewardQty_1 > 0)
        {
            AddRewardByTargetId(rewardData.Target_Id_1, rewardData.RewardQty_1);
        }

        if(rewardData.Target_Id_2 != 0 && rewardData.RewardQty_2 > 0)
        {
            AddRewardByTargetId(rewardData.Target_Id_2, rewardData.RewardQty_2);
        }

        if(rewardData.Target_Id_3 != 0 && rewardData.RewardQty_3 > 0)
        {
            AddRewardByTargetId(rewardData.Target_Id_3, rewardData.RewardQty_3);
        }
    }

    private void AddRewardByTargetId(int targetId, int quantity)
    {
        if(targetId >= 300000 && targetId < 400000) // 행성
        {
            UserDataMapper.AddPlanet(targetId);
            Debug.Log($"[GameResult] 행성 보상: ID={targetId}");
        }
        else if(targetId == 711101) // 골드
        {
            UserData.Gold += quantity;
            Debug.Log($"[GameResult] 골드 보상: +{quantity}");
        }
        else if(targetId == 711201) // 무료 다이아
        {
            UserData.FreeDia += quantity;
            Debug.Log($"[GameResult] 무료 다이아 보상: +{quantity}");
        }
        else if(targetId == 711202) // 유료 다이아
        {
            UserData.ChargedDia += quantity;
            Debug.Log($"[GameResult] 유료 다이아 보상: +{quantity}");
        }
        else if(targetId >= 710000 && targetId < 711000) // 아이템
        {
            UserDataMapper.AddItem(targetId, quantity);
            Debug.Log($"[GameResult] 아이템 보상: ID={targetId}, Qty={quantity}");
        }
        else
        {
            Debug.LogWarning($"[GameResult] 알 수 없는 보상 ID: {targetId}");
        }
    }

    private async UniTask SaveAllDataToFirebase()
    {
        CurrencyManager.Instance.MarkDirty();
        await CurrencyManager.Instance.SaveCurrencyAsync();

        await ItemManager.Instance.SaveItemsAsync();

        await PlanetManager.Instance.SavePlanetsAsync();
    }

    private async UniTask ProcessWaveGold()
    {
        await UniTask.WaitUntil(() => CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized);

        int waveGold = WaveManager.Instance.AccumulateGold;
        if(waveGold > 0)
        {
            UserData.Gold += waveGold;
        }
    }
}
