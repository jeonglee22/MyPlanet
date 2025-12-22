using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpPopUpUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField nickNameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    
    [SerializeField] private MainTitleCanvasManager canvasManager;

    [SerializeField] private Button signUpButton;
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private LogInPopUpUI logInPopUpUI;

    private string email = string.Empty;
    private string password = string.Empty;
    private string nickName = string.Empty;

    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);

        emailInputField.onValueChanged.AddListener(OnEmailValueChanged);
        passwordInputField.onValueChanged.AddListener(OnPasswordValueChanged);
        nickNameInputField.onValueChanged.AddListener(OnNickNameValueChanged);

        guestLoginButton.onClick.AddListener(() => OnGuestLoginButtonClicked().Forget());
        signUpButton.onClick.AddListener(() => OnSignUpButtonClicked().Forget());
        closeButton.onClick.AddListener(() => canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.LogIn));

        guestLoginButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        signUpButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        closeButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());

        InteractableButtons(true);
    }

    private void InteractableButtons(bool interactable)
    {
        guestLoginButton.interactable = interactable;
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

    private void OnNickNameValueChanged(string value)
    {
        nickName = value;
    }

    private async UniTaskVoid OnGuestLoginButtonClicked()
    {
        InteractableButtons(false);

        var result = await AuthManager.Instance.SignInAnonymousAsync();
        canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);
        // logInPopUpUI.OnNickNameValueChanged(AuthManager.Instance.UserNickName);
        // if (result)
        // {
        //     canvasManager.UpdateUIDText();
        // }

        InteractableButtons(true);
    }

    private async UniTaskVoid OnSignUpButtonClicked()
    {
        InteractableButtons(false);

        await AuthManager.Instance.CreateAccountWithEmailAsync(email, password, nickName);
        canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.LogIn);

        InteractableButtons(true);
    }
}
