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
}
