using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainTitleUI : MonoBehaviour
{
    // [SerializeField] private Button gameStartButton;
    [SerializeField] private Button logInOutButton;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject exitPanel;
    [SerializeField] private GameObject SignInPanel;
    [SerializeField] private Button exitCanelButton;
    [SerializeField] private Button exitAcceptButton;
    [SerializeField] private TextMeshProUGUI explainText;
    [SerializeField] private GameObject playText;
    [SerializeField] private InfoPopUpUI infoPopUpUI;
    // [SerializeField] private Button enemyTestButton;
    // [SerializeField] private Button cameraTestButton;
    // [SerializeField] private Button asyncRaidTestButton;
    [SerializeField] private MainTitleCanvasManager canvasManager;

    [SerializeField] private TextMeshProUGUI uidText;

    [SerializeField] private LoadManager loadManager;
    private bool isNotGameStart = true;
    private bool finishLoading;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        // SetResolution();

        playText.SetActive(false);

        await CheckLogin();

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);

        // canvasManager.UpdateUIDText();

        // gameStartButton?.onClick.AddListener(() => OnStartGameButtonClicked().Forget());
        logInOutButton?.onClick.AddListener(() => OnLogInOutButtonClicked());
        // enemyTestButton?.onClick.AddListener(() => OnEnemyTestButtonClicked().Forget());
        // cameraTestButton?.onClick.AddListener(() => OnCameraTestButtonClicked().Forget());
        // asyncRaidTestButton?.onClick.AddListener(() => OnAsyncRaidTestButtonclicked().Forget());

        exitButton?.onClick.AddListener(() => exitPanel.SetActive(true));
        exitCanelButton?.onClick.AddListener(() => exitPanel.SetActive(false));

#if UNITY_EDITOR
        exitAcceptButton?.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#elif UNITY_ANDROID
        exitAcceptButton?.onClick.AddListener(() => Application.Quit());
#endif

        InteractableButtons(true);

        exitPanel.SetActive(false);

        finishLoading = true;

        // gameStartButton.interactable = false;
    }

    // private void SetResolution()
    // {
    //     int setWidth = 2100 * 9 / 20;
    //     int setHeight = 2100;

    //     Screen.SetResolution(setWidth, setHeight, false);
    // }

    private async UniTask CheckLogin()
    {
        SetExplainText("Checking login status...");

        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        if (AuthManager.Instance.IsSignedIn)
        {
            await CheckAllDataUpdated();

            SetExplainText($"Welcome back, {AuthManager.Instance.UserNickName}!");
            infoPopUpUI.SetNickNameText(AuthManager.Instance.UserNickName);
            playText.SetActive(true);
        }
        else
        {
            SetExplainText("Please log in to continue.");
            canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.LogIn);
        }
    }

    private async UniTask CheckAllDataUpdated()
    {
        SetExplainText("Updating user data...");

        await UserPlanetManager.Instance.LoadUserPlanetAsync();

        // await CurrencyManager.Instance.LoadCurrencyAsync();

        await UserTowerManager.Instance.LoadUserTowerDataAsync();
    }

    private void OnLogInOutButtonClicked()
    {
        if(AuthManager.Instance.IsSignedIn)
        {
            canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.Info);
            // logInOutButton.GetComponentInChildren<TextMeshProUGUI>().text = "Log In";
            // canvasManager.UpdateUIDText();
            // gameStartButton.interactable = false;
        }
        else
        {
            canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.LogIn);
            // logInOutButton.GetComponentInChildren<TextMeshProUGUI>().text = "Log Out";
            // canvasManager.UpdateUIDText();
            // gameStartButton.interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!finishLoading) return;

        if(AuthManager.Instance.IsSignedIn)
        {
            if (TouchManager.Instance.IsTouching && isNotGameStart && CheckAllPanelClosed() &&
                !RectTransformUtility.RectangleContainsScreenPoint(logInOutButton.GetComponent<RectTransform>(), TouchManager.Instance.TouchPos) &&
                !RectTransformUtility.RectangleContainsScreenPoint(exitButton.GetComponent<RectTransform>(), TouchManager.Instance.TouchPos))
            {
                Debug.Log("Game Start Button Clicked");
                isNotGameStart = false;
                SceneControlManager.Instance.LoadScene(SceneName.LobbyScene).Forget();
            }
        }
    }

    private bool CheckAllPanelClosed()
    {
        return !exitPanel.activeSelf && !infoPanel.activeSelf && !SignInPanel.activeSelf;
    }

    private void InteractableButtons(bool interactable)
    {
        // gameStartButton.interactable = interactable;
        // signUpButton.interactable = interactable;
        // closeButton.interactable = interactable;
    }

    public void SetExplainText(string message)
    {
        explainText.text = message;
    }

    public void SetActivePlayText(bool active)
    {
        playText.SetActive(active);
    }

    // private async UniTaskVoid OnStartGameButtonClicked()
    // {
    //     // List<UniTask> loadTasks = new List<UniTask>
    //     // {
    //     //     loadManager.LoadGamePrefabAsync(AddressLabel.Prefab),
    //     // };

    //     Debug.Log("Game Start Button Clicked");
    //     await SceneControlManager.Instance.LoadScene(SceneName.StageSelectScene);
    // }

    // private async UniTaskVoid OnEnemyTestButtonClicked()
    // {
    //     await SceneControlManager.Instance.LoadScene(SceneName.EnemyTestScene);
    // }

    // private async UniTaskVoid OnCameraTestButtonClicked()
    // {
    //     await SceneControlManager.Instance.LoadScene(SceneName.CameraTestScene);
    // }

    // private async UniTaskVoid OnAsyncRaidTestButtonclicked()
    // {
    //     await SceneControlManager.Instance.LoadScene(SceneName.AsyncRaidTestScene);
    // }
}
