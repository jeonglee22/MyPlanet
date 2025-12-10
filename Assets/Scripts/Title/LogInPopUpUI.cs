using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInPopUpUI : MonoBehaviour
{
    [SerializeField] private InputField nickNameInputField;
    
    [SerializeField] private MainTitleCanvasManager canvasManager;

    [SerializeField] private Button signUpButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text errorMessageText;
    [SerializeField] private InfoPopUpUI infoPopUpUI;
    [SerializeField] private MainTitleUI mainTitleUI;

    private string nickName = string.Empty;

    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);

        nickNameInputField.onValueChanged.AddListener(OnNickNameValueChanged);

        // loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signUpButton.onClick.AddListener(() => OnSignInButtonClicked());
        closeButton.onClick.AddListener(() => canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None));

        SetErrorMessage(string.Empty);

        InteractableButtons(true);
    }

    private void InteractableButtons(bool interactable)
    {
        signUpButton.interactable = interactable;
        closeButton.interactable = interactable;
    }

    private void OnNickNameValueChanged(string value)
    {
        nickName = value;
    }

    private async void OnSignInButtonClicked()
    {
        if (nickName == string.Empty)
        {
            SetErrorMessage("Please enter a nickname.");
            return;
        }
        
        InteractableButtons(false);

        var result = await AuthManager.Instance.SignInAnonymousAsync(nickName);

        if (result == false)
        {
            SetErrorMessage("Anonymous sign-in failed.");
            InteractableButtons(true);
            return;
        }

        var uploadNickName = await UserPlanetManager.Instance.InitUserPlanetAsync(nickName);

        if (uploadNickName == false)
        {
            SetErrorMessage("Failed to save nickname.");
            InteractableButtons(true);
            return;
        }

        var initTowerData = await UserTowerManager.Instance.InitUserTowerDataAsync();
        if (initTowerData == false)
        {
            SetErrorMessage("Failed to initialize tower data.");
            InteractableButtons(true);
            return;
        }

        canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);
        infoPopUpUI.SetNickNameText(AuthManager.Instance.UserNickName);

        mainTitleUI.SetExplainText($"Welcome, {AuthManager.Instance.UserNickName}!");
        mainTitleUI.SetActivePlayText(true);

        InteractableButtons(true);
    }

    public void SetErrorMessage(string message)
    {
        errorMessageText.text = message;
    }

    // private async UniTaskVoid OnLoginButtonClicked()
    // {
    //     InteractableButtons(false);

    //     var result = await AuthManager.Instance.SignInWithEmailAsync(email, password);
    //     canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);
    //     // if (result)
    //     // {
    //     //     canvasManager.UpdateUIDText();
    //     // }

    //     InteractableButtons(true);
    // }
}