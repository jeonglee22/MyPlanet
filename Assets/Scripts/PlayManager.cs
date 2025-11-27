using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private Planet planet;
    [SerializeField] private WaveManager waveManager;

    [SerializeField] private GameObject gameOverUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.OnDeathEvent += GameOver;
        gameOverUI.SetActive(false);
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
        Time.timeScale = 0f;
    }

    private void GameClear()
    {
        gameOverUI.GetComponent<GameResultUI>().SetResultText(true);
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }
}
