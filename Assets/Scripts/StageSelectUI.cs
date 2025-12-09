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

    private void Start()
    {
        InitializeStageButtons();

        enemyTestSceneButton.onClick.AddListener(() => SceneControlManager.Instance.LoadScene(SceneName.EnemyTestScene).Forget());
        balanceTestSceneButton.onClick.AddListener(() => 
        {
            Variables.IsTestMode = true;
            SceneControlManager.Instance.LoadScene(SceneName.BalanceTestScene).Forget();
        });
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
}
