using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private Planet planet;
    [SerializeField] private WaveManager waveManager;

    [SerializeField] private GameObject gameOverUI;

    private bool isTutorial = false;

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
        if (waveManager.IsCleared)
        {
            GameClear();
        }
    }
    
    private void GameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI?.GetComponent<GameResultUI>().SetResultText(false);
            gameOverUI?.SetActive(true);
        }
        
        GamePauseManager.Instance.Pause();
    }

    private void GameClear()
    {
        if (Variables.IsTestMode)
        {
            return;
        }

        if(isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(12);
        }

        if (gameOverUI != null)
        {
            gameOverUI?.GetComponent<GameResultUI>().SetResultText(true);
            gameOverUI?.SetActive(true);
        }
        
        GamePauseManager.Instance.Pause();
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }
}
