using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class SceneControlManager : MonoBehaviour
{
    private static SceneControlManager instance;
    public static SceneControlManager Instance => instance;

    private string currentSceneName;
    public string CurrentSceneName { get => currentSceneName; set => currentSceneName = value; }

    [SerializeField] private GameObject loadingPanelObject;
    private GameObject loadingPanel;
    public GameObject LoadingPanel { get => loadingPanel; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        Debug.Log(loadingPanelObject == null);
        loadingPanel = Instantiate(instance.loadingPanelObject, GameObject.FindWithTag(TagName.MainCanvas).transform);

        await FireBaseInitializer.Instance.WaitInitialization();

        loadingPanel.SetActive(false);

        currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        if (loadingPanel == null)
        {
            loadingPanel = Instantiate(instance.loadingPanelObject, GameObject.FindWithTag(TagName.MainCanvas).transform);
        }
    }

    public async UniTask LoadScene(string sceneName, List<UniTask> additionalTasks = null)
    {
        loadingPanel.SetActive(true);
        // Time.timeScale = 0f;

        var sceneLoad = Addressables.LoadSceneAsync(SceneName.LoadingScene).ToUniTask();

        var tasks = new List<UniTask>() { sceneLoad, WaitSceneLoadMinimun(500) };

        await UniTask.WhenAll(tasks);

        Debug.Log("LoadingScene Loaded");

        tasks = additionalTasks ?? new List<UniTask>();
        tasks.Add(WaitSceneLoadMinimun(2000));

        await UniTask.WhenAll(tasks);

        var newSceneLoad = Addressables.LoadSceneAsync(sceneName).ToUniTask();

        tasks = new List<UniTask>() { newSceneLoad, WaitSceneLoadMinimun(500) };

        await UniTask.WhenAll(tasks);

        Debug.Log("Scene Loaded");

        currentSceneName = sceneName;
        loadingPanel.SetActive(false);
        // Time.timeScale = 1f;
    }

    public async UniTask WaitSceneLoadMinimun(float waitTime)
    {
        await UniTask.Delay(TimeSpan.FromMilliseconds(waitTime), DelayType.UnscaledDeltaTime);
    }
}
