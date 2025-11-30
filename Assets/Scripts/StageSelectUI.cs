using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button stageButtonSample;

    private void Start()
    {
        InitializeStageButtons();
    }

    private void InitializeStageButtons()
    {
        int stageCount = DataTableManager.WaveTable.GetStageCount();

        Transform content = scrollRect.content;

        for(int i = 0; i < stageCount; i++)
        {
            int stageIndex = i;

            Button stageButton = Instantiate(stageButtonSample, content);
            stageButton.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = stageButton.GetComponentInChildren<TextMeshProUGUI>();
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
