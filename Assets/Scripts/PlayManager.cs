using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private Planet planet;
    [SerializeField] private WaveManager waveManager;

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private BattleUI battleUI;

    private bool isTutorial = false;

    private bool hasEnded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.OnDeathEvent += GameOver;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (Variables.IsTestMode)
        {
            return;
        }
        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
    }

    void OnDestroy()
    {
        planet.OnDeathEvent -= GameOver;
    }

    private void Update()
    {
        if (hasEnded) return;

        if (waveManager.IsCleared)
        {
            GameClear();
        }
    }

    private void GameOver()
    {
        if (hasEnded) return;
        hasEnded = true;

        SoundManager.Instance.PlayDefeatSound();

        if (gameOverUI != null)
        {
            gameOverUI.GetComponent<GameResultUI>()?.SetResultText(false);
            gameOverUI.SetActive(true);
            gameOverUI.GetComponent<GameResultUI>()?.SetGameResultText(false, Variables.Stage, battleUI.TimeText.text, battleUI.EnemyKiilCount);
        }

        GamePauseManager.Instance.Pause();
    }

    private async void GameClear()
    {
        if (Variables.IsTestMode)
            return;

        if (hasEnded) return;
        hasEnded = true;

        if (isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(12);
        }

        SoundManager.Instance.PlayVictorySound();

        var currentLastCelearedStage = UserStageManager.Instance.ClearedStageData.HighestClearedStage;
        if (Variables.Stage == currentLastCelearedStage)
        {
            var result = await UserStageManager.Instance.SaveUserStageClearAsync(Variables.Stage + 1);
            if (!result)
            {
                Debug.LogError("Failed to save stage clear data.");
            }
        }

        if (gameOverUI != null)
        {
            gameOverUI.GetComponent<GameResultUI>()?.SetResultText(true);
            gameOverUI.SetActive(true);
            gameOverUI.GetComponent<GameResultUI>()?.SetGameResultText(true, Variables.Stage, battleUI.TimeText.text, battleUI.EnemyKiilCount);
        }

        GamePauseManager.Instance.Pause();
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }
}
