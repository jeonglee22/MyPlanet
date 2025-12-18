using UnityEngine;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private Planet planet;
    [SerializeField] private WaveManager waveManager;

    [SerializeField] private GameObject gameOverUI;

    private bool isTutorial = false;

    [Header("Result SFX")]
    [SerializeField] private AudioSource resultAudioSource;
    [SerializeField] private AudioClip clearSfx;
    [SerializeField] private AudioClip failSfx;
    [SerializeField, Range(0f, 1f)] private float resultSfxVolume = 1f;
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

        PlayResultSfx(failSfx);

        if (gameOverUI != null)
        {
            gameOverUI.GetComponent<GameResultUI>()?.SetResultText(false);
            gameOverUI.SetActive(true);
        }

        GamePauseManager.Instance.Pause();
    }

    private void GameClear()
    {
        if (Variables.IsTestMode)
            return;

        if (hasEnded) return;
        hasEnded = true;

        if (isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(12);
        }

        PlayResultSfx(clearSfx);

        if (gameOverUI != null)
        {
            gameOverUI.GetComponent<GameResultUI>()?.SetResultText(true);
            gameOverUI.SetActive(true);
        }

        GamePauseManager.Instance.Pause();
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }

    private void EnsureResultAudioSource()
    {
        if (resultAudioSource != null) return;

        resultAudioSource = GetComponent<AudioSource>();
        if (resultAudioSource == null) resultAudioSource = gameObject.AddComponent<AudioSource>();

        resultAudioSource.playOnAwake = false;
        resultAudioSource.loop = false;
        resultAudioSource.spatialBlend = 0f; 
    }

    private void PlayResultSfx(AudioClip clip)
    {
        if (clip == null) return;

        EnsureResultAudioSource();
        resultAudioSource.PlayOneShot(clip, resultSfxVolume);
    }
}
