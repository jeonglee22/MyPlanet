using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInPopUpUI : MonoBehaviour
{
    [SerializeField] private InputField nickNameInputField;
    
    [SerializeField] private MainTitleCanvasManager canvasManager;

    [SerializeField] private Button signUpButton;
    [SerializeField] private Button googleLogInButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text errorMessageText;
    [SerializeField] private InfoPopUpUI infoPopUpUI;
    [SerializeField] private MainTitleUI mainTitleUI;

    private string nickName = string.Empty;

    private async UniTaskVoid Start()
    {
        InteractableButtons(false);

        await UniTask.WaitUntil(() => canvasManager.IsInitialized);
        await UniTask.WaitUntil(() => UserPlanetManager.Instance != null && UserPlanetManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        nickNameInputField.onValueChanged.AddListener(OnNickNameValueChanged);

        // loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        googleLogInButton.onClick.AddListener(() => OnGoogleLogInButtonClicked().Forget());

        signUpButton.onClick.AddListener(() => OnSignInButtonClicked().Forget());
        closeButton.onClick.AddListener(() => canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None));

        signUpButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        closeButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());

        SetErrorMessage(string.Empty).Forget();

        InteractableButtons(true);
    }

    private void InteractableButtons(bool interactable)
    {
        signUpButton.interactable = interactable;
        googleLogInButton.interactable = interactable;
        closeButton.interactable = interactable;
    }

    private void OnNickNameValueChanged(string value)
    {
        nickName = value;
    }

    private async UniTaskVoid OnGoogleLogInButtonClicked()
    {
        InteractableButtons(false);

        try
        {
            var result = await AuthManager.Instance.SignInWithGoogleAsync();

            if (result.Item1 == false)
            {
                SetErrorMessage($"Google sign-in failed. : {result.Item2}").Forget();
                Debug.LogError(result.Item2);
                AuthManager.Instance.SignOut();
                InteractableButtons(true);
                return;
            }

            var userRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserPlanets);
            var dataSnapshot = await userRef.Child(AuthManager.Instance.UserId).GetValueAsync().AsUniTask();

            if (!dataSnapshot.Exists)
            {
                nickName = AuthManager.Instance.UserNickName;

                await UserPlanetManager.Instance.InitUserPlanetAsync(nickName);
                await UserTowerManager.Instance.InitUserTowerDataAsync();
                await UserTowerUpgradeManager.Instance.InitUserTowerUpgradeAsync();
                await UserStageManager.Instance.InitUserStageClearAsync();
                await UserShopItemManager.Instance.InitUserShopItemDataAsync();

                await UserAttackPowerManager.Instance.UpdatePlanetPower(UserPlanetManager.Instance.CurrentPlanet);
                await UserAttackPowerManager.Instance.UpdateTowerPower();
            }
            canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);
            infoPopUpUI.SetNickNameText(AuthManager.Instance.UserNickName);

            mainTitleUI.SetExplainText($"Welcome back, {AuthManager.Instance.UserNickName}!");
            mainTitleUI.SetActivePlayText(true);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Google Log-In Error: {ex.Message}");
            throw;
        }

        
        InteractableButtons(true);
    }

    private async UniTaskVoid OnSignInButtonClicked()
    {
        if (nickName == string.Empty)
        {
            SetErrorMessage("Please enter a nickname.").Forget();
            return;
        }
        
        InteractableButtons(false);

        var result = await AuthManager.Instance.SignInAnonymousAsync(nickName);

        if (result == false)
        {
            SetErrorMessage("Anonymous sign-in failed.").Forget();
            InteractableButtons(true);
            return;
        }

        var uploadNickName = await UserPlanetManager.Instance.InitUserPlanetAsync(nickName);

        if (uploadNickName == false)
        {
            SetErrorMessage("Failed to save nickname.").Forget();
            InteractableButtons(true);
            AuthManager.Instance.SignOut();
            return;
        }

        var initTowerData = await UserTowerManager.Instance.InitUserTowerDataAsync();
        if (initTowerData == false)
        {
            SetErrorMessage("Failed to initialize tower data.").Forget();
            InteractableButtons(true);
            AuthManager.Instance.SignOut();
            return;
        }

        var initUpgradeTowerData = await UserTowerUpgradeManager.Instance.InitUserTowerUpgradeAsync();
        if (initUpgradeTowerData == false)
        {
            SetErrorMessage("Failed to initialize tower upgrade data.").Forget();
            InteractableButtons(true);
            AuthManager.Instance.SignOut();
            return;
        }

        var initUserStageData = await UserStageManager.Instance.InitUserStageClearAsync();
        if (initUserStageData == false)
        {
            SetErrorMessage("Failed to initialize user stage data.").Forget();
            InteractableButtons(true);
            AuthManager.Instance.SignOut();
            return;
        }

        var initUserShopItemData = await UserShopItemManager.Instance.InitUserShopItemDataAsync();
        if (initUserShopItemData == false)
        {
            SetErrorMessage("Failed to initialize user shop item data.").Forget();
            InteractableButtons(true);
            AuthManager.Instance.SignOut();
            return;
        }

        await UserAttackPowerManager.Instance.UpdatePlanetPower(UserPlanetManager.Instance.CurrentPlanet);
        await UserAttackPowerManager.Instance.UpdateTowerPower();

        canvasManager.SwitchToTargetPopUp(MainTitleCanvasManager.PopupName.None);
        infoPopUpUI.SetNickNameText(AuthManager.Instance.UserNickName);

        mainTitleUI.SetExplainText($"Welcome, {AuthManager.Instance.UserNickName}!");
        mainTitleUI.SetActivePlayText(true);

        InteractableButtons(true);
    }

    public async UniTaskVoid SetErrorMessage(string message)
    {
        errorMessageText.text = message;
        await UniTask.WaitForSeconds(1);
        errorMessageText.text = string.Empty;
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