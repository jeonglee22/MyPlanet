using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    // [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button[] stageButtons;
    [SerializeField] private Button enemyTestSceneButton; 
    [SerializeField] private Button balanceTestSceneButton;
    [SerializeField] private Button backBtn;

    private void Start()
    {
        ResetBtn();
        InitializeStageButtons();

        enemyTestSceneButton.onClick.AddListener(() => SceneControlManager.Instance.LoadScene(SceneName.EnemyTestScene).Forget());
        balanceTestSceneButton.onClick.AddListener(() => 
        {
            Variables.IsTestMode = true;
            SceneControlManager.Instance.LoadScene(SceneName.BalanceTestScene).Forget();
        });
        backBtn.onClick.AddListener(OnBackBtnClicked);
    }

    private void OnDestroy()
    {
        ResetBtn();
    }

    private void ResetBtn()
    {
        enemyTestSceneButton.onClick.RemoveAllListeners();
        balanceTestSceneButton.onClick.RemoveAllListeners();
        backBtn.onClick.RemoveListener(OnBackBtnClicked);
    }

    private void InitializeStageButtons()
    {
        int stageCount = DataTableManager.WaveTable.GetStageCount();

        for(int i = 0; i < stageCount; i++)
        {
            int stageIndex = i;
            Button stageButton = stageButtons[stageIndex];

            TextMeshProUGUI buttonText = stageButton.GetComponent<TextMeshProUGUI>();
            if(buttonText != null)
            {
                buttonText.text = $"Stage {stageIndex + 1}";
            }

            stageButton.onClick.AddListener(() => OnStageBUttonClicked(stageIndex));
        }
    }

    private void OnStageBUttonClicked(int stageIndex)
    {
        Variables.Stage = stageIndex + 1;
        LoadStageScene().Forget();
    }

    private async UniTaskVoid LoadStageScene()
    {
        await SceneControlManager.Instance.LoadScene(SceneName.BattleScene);
    }

    private void OnBackBtnClicked()
    {
        SceneControlManager.Instance.LoadScene(SceneName.LobbyScene).Forget();
    }
}
