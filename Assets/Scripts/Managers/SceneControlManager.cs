using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

public class SceneControlManager : MonoBehaviour
{
    private static SceneControlManager instance;
    public static SceneControlManager Instance => instance;

    private string currentSceneName;
    public string CurrentSceneName { get => currentSceneName; set => currentSceneName = value; }
    public bool IsLoading { get; private set; }

    [SerializeField] private GameObject loadingCanvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        Debug.Log(loadingCanvas == null);

        await FireBaseInitializer.Instance.WaitInitialization();

        currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
    }

    public async UniTask LoadScene(string sceneName, List<UniTask> additionalTasks = null)
    {
        loadingCanvas.SetActive(true);
        IsLoading = true;
        // Time.timeScale = 0f;

        var sceneLoad = Addressables.LoadSceneAsync(SceneName.LoadingScene).ToUniTask();

        var tasks = new List<UniTask>() { sceneLoad };

        await UniTask.WhenAll(tasks);

        Debug.Log("LoadingScene Loaded");

        tasks = additionalTasks ?? new List<UniTask>();
        tasks.Add(WaitSceneLoadMinimun(1000));

        await UniTask.WhenAll(tasks);

        var newSceneLoad = Addressables.LoadSceneAsync(sceneName).ToUniTask();

        tasks = new List<UniTask>() { newSceneLoad };

        await UniTask.WhenAll(tasks);

        IsLoading = false;

        Debug.Log("Scene Loaded");

        currentSceneName = sceneName;
        loadingCanvas.SetActive(false);
        // Time.timeScale = 1f;
    }

    public async UniTask WaitSceneLoadMinimun(float waitTime)
    {
        await UniTask.Delay(TimeSpan.FromMilliseconds(waitTime), DelayType.UnscaledDeltaTime);
    }
}
