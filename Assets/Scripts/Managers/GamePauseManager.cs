using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    private static GamePauseManager instance;
    public static GamePauseManager Instance => instance;

    public bool IsGamePaused { get; private set; } = false;

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
        if (IsGamePaused)
        {
            return;
        }

        IsGamePaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!IsGamePaused)
        {
            return;
        }

        IsGamePaused = false;
        Time.timeScale = 1f;
    }
}
