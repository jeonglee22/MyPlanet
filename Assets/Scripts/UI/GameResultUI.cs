using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
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

    [SerializeField] private List<Image> dropItemImages;
    [SerializeField] private List<TextMeshProUGUI> dropItemTexts;

    void Start()
    {
        WaveManager.Instance.Cancel();
        // restartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        restartButton?.onClick.AddListener(OnRestartCliecked);
        returnToTitleButton?.onClick.AddListener(OnReturnToTitleClicked);
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
