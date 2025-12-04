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
        gameOverUI.SetActive(false);

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
        gameOverUI.GetComponent<GameResultUI>().SetResultText(false);
        gameOverUI.SetActive(true);
        GamePauseManager.Instance.Pause();
    }

    private void GameClear()
    {
        if(isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(12);
        }

        gameOverUI.GetComponent<GameResultUI>().SetResultText(true);
        gameOverUI.SetActive(true);
        GamePauseManager.Instance.Pause();
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }
}
