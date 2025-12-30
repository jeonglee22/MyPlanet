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

    [SerializeField] private Planet planet;

    private StageData currentStageData;
    private bool isGameClear = false;

    [SerializeField] private List<GameObject> rewardObj;
    private List<Image> itemImages = new List<Image>();
    private List<TextMeshProUGUI> item1Texts = new List<TextMeshProUGUI>();

    void Start()
    {
        WaveManager.Instance.Cancel();
        // restartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        restartButton?.onClick.AddListener(OnRestartCliecked);
        checkButton?.onClick.AddListener(OnReturnToTitleClicked);
        detailButton?.onClick.AddListener(OnOpenDetailPanelClicked);

        foreach(var obj in rewardObj)
        {
            itemImages.Add(obj.transform.GetChild(0).GetComponent<Image>());
            item1Texts.Add(obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>());
        }
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

        if(isGameClear && currentStageData != null)
        {
            int rewardId = isFirstClear ? currentStageData.FirstReward_Id : currentStageData.Reward_Id;

            var rewardData = DataTableManager.StageRewardTable.Get(rewardId);
            if(rewardData != null)
            {
                ProcessStageRewardData(rewardData);
            }
        }

        await SaveAllDataToFirebase();

        if (isGameClear && isFirstClear)
        {
            await UserStageManager.Instance.SaveUserStageClearAsync(Variables.Stage + 1);
        }
    }

    private void ProcessStageRewardData(StageRewardData rewardData)
    {
        int waveGold = WaveManager.Instance.AccumulateGold;
        
    }

    private void AddRewardByTargetId(int targetId, int quantity, int index)
    {
        if(targetId == 711101)
        {
            UserData.Gold += quantity;
            itemImages[index].sprite = LoadManager.GetLoadedGameTexture(DataTableManager.CurrencyTable.Get(targetId).CurrencyIconText);
        }
        else if(targetId == 711201)
        {
            UserData.FreeDia += quantity;
            itemImages[index].sprite = LoadManager.GetLoadedGameTexture(DataTableManager.CurrencyTable.Get(targetId).CurrencyIconText);
        }
        else if(targetId == 711202)
        {
            UserData.ChargedDia += quantity;
            itemImages[index].sprite = LoadManager.GetLoadedGameTexture(DataTableManager.CurrencyTable.Get(targetId).CurrencyIconText);
        }
        else
        {
            UserDataMapper.AddItem(targetId, quantity);
            itemImages[index].sprite = LoadManager.GetLoadedGameTexture(DataTableManager.ItemTable.Get(targetId).ItemIconText);
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
