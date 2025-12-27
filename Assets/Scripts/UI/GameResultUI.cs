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

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI item1Text;
    [SerializeField] private TextMeshProUGUI item2Text;
    [SerializeField] private TextMeshProUGUI item3Text;

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

        ProcessRewards().Forget();
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

    private async UniTask ProcessRewards()
    {
        await UniTask.WaitUntil(() => CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => UserStageManager.Instance != null && UserStageManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => ItemManager.Instance != null && ItemManager.Instance.IsInitialized);

        int highestCleared = UserStageManager.Instance.ClearedStageData.HighestClearedStage;
        bool isFirstClear = Variables.Stage >= highestCleared;
        Debug.Log($"[GameResult] 최고 클리어: {highestCleared}, 현재 스테이지: {Variables.Stage}, 최초 클리어: {isFirstClear}");

        int waveGold = WaveManager.Instance.AccumulateGold;
        Debug.Log($"[GameResult] 웨이브 누적 골드: {waveGold}");
        if(waveGold > 0)
        {
            UserData.Gold += waveGold;
            Debug.Log($"[GameResult] 웨이브 골드 추가 후: {UserData.Gold}");
        }

        if(isGameClear && currentStageData != null)
        {
            int rewardId = isFirstClear ? currentStageData.FirstReward_Id : currentStageData.Reward_Id;
            Debug.Log($"[GameResult] 보상 ID: {rewardId}");

            var rewardData = DataTableManager.StageRewardTable.Get(rewardId);
            if(rewardData != null)
            {
                Debug.Log($"[GameResult] 보상 데이터 찾음 - Target1: {rewardData.Target_Id_1}, Qty1: {rewardData.RewardQty_1}");
                Debug.Log($"[GameResult] 보상 데이터 - Target2: {rewardData.Target_Id_2}, Qty2: {rewardData.RewardQty_2}");
                Debug.Log($"[GameResult] 보상 데이터 - Target3: {rewardData.Target_Id_3}, Qty3: {rewardData.RewardQty_3}");

                ProcessStageRewardData(rewardData);
                Debug.Log($"[GameResult] 스테이지 보상 추가 후 골드: {UserData.Gold}");
            }
        }

        Debug.Log($"[GameResult] Firebase 저장 전 최종 골드: {UserData.Gold}");
        await SaveAllDataToFirebase();

        if (isGameClear && isFirstClear)
        {
            await UserStageManager.Instance.SaveUserStageClearAsync(Variables.Stage + 1);
        }
    }

    private void ProcessStageRewardData(StageRewardData rewardData)
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
        if(targetId == 711101)
        {
            UserData.Gold += quantity;
            Debug.Log($"[GameResult] 골드 보상: +{quantity}");
        }
        else if(targetId == 711201)
        {
            UserData.FreeDia += quantity;
            Debug.Log($"[GameResult] 무료 다이아 보상: +{quantity}");
        }
        else if(targetId == 711202)
        {
            UserData.ChargedDia += quantity;
            Debug.Log($"[GameResult] 유료 다이아 보상: +{quantity}");
        }
        else
        {
            UserDataMapper.AddItem(targetId, quantity);
            Debug.Log($"[GameResult] 아이템 보상: ID={targetId}, Qty={quantity}");
        }
    }

    private async UniTask SaveAllDataToFirebase()
    {
        CurrencyManager.Instance.MarkDirty();
        await CurrencyManager.Instance.SaveCurrencyAsync();

        await ItemManager.Instance.SaveItemsAsync();

        await PlanetManager.Instance.SavePlanetsAsync();
    }
}
