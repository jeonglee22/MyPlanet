using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    private static GamePauseManager instance;
    public static GamePauseManager Instance => instance;

    public bool IsGamePaused { get; private set; } = false;

    private int pauseCount = 0;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Pause()
    {
        pauseCount++;

        if(pauseCount == 1)
        {
            IsGamePaused = true;
            Time.timeScale = 0f;
        }
    }

    public void Resume()
    {
        pauseCount--;

        if(pauseCount < 0)
        {
            pauseCount = 0;
        }

        if(pauseCount == 0)
        {
            IsGamePaused = false;
            Time.timeScale = 1f;
        }
    }
}
