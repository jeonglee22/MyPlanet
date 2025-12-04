using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopUpUI : MonoBehaviour
{
    [SerializeField] private Text nickNameText;
    
    [SerializeField] private MainTitleCanvasManager canvasManager;
    [SerializeField] private MainTitleUI mainTitleUI;

    [SerializeField] private Button logOutButton;
    [SerializeField] private Button closeButton;

    private string nickName = string.Empty;

    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);

        // nickNameInputField.onValueChanged.AddListener(OnNickNameValueChanged);

        // loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        logOutButton.onClick.AddListener(() => OnLogOutButtonClicked());
        closeButton.onClick.AddListener(() => canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None));

        InteractableButtons(true);
    }

    public void SetNickNameText(string nickName)
    {   
        nickNameText.text = nickName;
    }

    public void SetAttackPowerText(int attackPower)
    {
        // attackPowerText.text = attackPower.ToString();
    }

    private void InteractableButtons(bool interactable)
    {
        logOutButton.interactable = interactable;
        closeButton.interactable = interactable;
    }

    private void OnLogOutButtonClicked()
    {
        InteractableButtons(false);

        AuthManager.Instance.SignOut();

        canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);

        mainTitleUI.SetExplainText("Please log in to continue.");
        mainTitleUI.SetActivePlayText(false);

        InteractableButtons(true);
    }
}
