using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class MainTitleUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button logInOutButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button enemyTestButton;
    [SerializeField] private Button cameraTestButton;
    [SerializeField] private MainTitleCanvasManager canvasManager;

    [SerializeField] private TextMeshProUGUI uidText;

    [SerializeField] private LoadManager loadManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);

        canvasManager.UpdateUIDText();

        gameStartButton.onClick.AddListener(() => OnStartGameButtonClicked().Forget());
        logInOutButton.onClick.AddListener(() => OnLogInOutButtonClicked());
        enemyTestButton.onClick.AddListener(() => OnEnemyTestButtonClicked().Forget());
        cameraTestButton.onClick.AddListener(() => OnCameraTestButtonClicked().Forget());

#if UNITY_EDITOR
        exitButton.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#elif UNITY_ANDROID
        exitButton.onClick.AddListener(() => Application.Quit());
#endif

        InteractableButtons(true);

        gameStartButton.interactable = false;
    }

    private void OnLogInOutButtonClicked()
    {
        if(AuthManager.Instance.IsSignedIn)
        {
            AuthManager.Instance.SignOut();
            logInOutButton.GetComponentInChildren<TextMeshProUGUI>().text = "Log In";
            canvasManager.UpdateUIDText();
            gameStartButton.interactable = false;
        }
        else
        {
            canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.LogIn);
            logInOutButton.GetComponentInChildren<TextMeshProUGUI>().text = "Log Out";
            canvasManager.UpdateUIDText();
            gameStartButton.interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(AuthManager.Instance.IsSignedIn)
        {
            gameStartButton.interactable = true;
        }
    }

    private void InteractableButtons(bool interactable)
    {
        gameStartButton.interactable = interactable;
        // signUpButton.interactable = interactable;
        // closeButton.interactable = interactable;
    }

    private async UniTaskVoid OnStartGameButtonClicked()
    {
        // List<UniTask> loadTasks = new List<UniTask>
        // {
        //     loadManager.LoadGamePrefabAsync(AddressLabel.Prefab),
        // };

        Debug.Log("Game Start Button Clicked");
        await SceneControlManager.Instance.LoadScene(SceneName.UiTestScene);
    }

    private async UniTaskVoid OnEnemyTestButtonClicked()
    {


        await SceneControlManager.Instance.LoadScene(SceneName.EnemyTestScene);
    }

    private async UniTaskVoid OnCameraTestButtonClicked()
    {
        await SceneControlManager.Instance.LoadScene(SceneName.CameraTestScene);
    }
}
