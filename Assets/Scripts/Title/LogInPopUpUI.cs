using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInPopUpUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    
    [SerializeField] private MainTitleCanvasManager canvasManager;

    [SerializeField] private Button loginButton;
    [SerializeField] private Button signUpButton;
    [SerializeField] private Button closeButton;

    private string email = string.Empty;
    private string password = string.Empty;

    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);

        emailInputField.onValueChanged.AddListener(OnEmailValueChanged);
        passwordInputField.onValueChanged.AddListener(OnPasswordValueChanged);

        loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signUpButton.onClick.AddListener(() => canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.SignUp));
        closeButton.onClick.AddListener(() => canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None));

        InteractableButtons(true);
    }

    private void InteractableButtons(bool interactable)
    {
        loginButton.interactable = interactable;
        signUpButton.interactable = interactable;
        closeButton.interactable = interactable;
    }

    private void OnEmailValueChanged(string value)
    {
        email = value;
    }

    private void OnPasswordValueChanged(string value)
    {
        password = value;
    }

    private async UniTaskVoid OnLoginButtonClicked()
    {
        InteractableButtons(false);

        var result = await AuthManager.Instance.SignInWithEmailAsync(email, password);
        canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);
        if (result)
        {
            canvasManager.UpdateUIDText();
        }

        InteractableButtons(true);
    }
}