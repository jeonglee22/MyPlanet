using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class MainTitleCanvasManager : MonoBehaviour
{
    public enum PopupName
    {
        None = -1,
        LogIn,
        Info
    }

    [SerializeField] private List<GameObject> popUpUIs;

    private PopupName currentPopUpUI = PopupName.None;

    private FirebaseUser user;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    [SerializeField] private TextMeshProUGUI uidText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        user = AuthManager.Instance.CurrentUser;

        Debug.Log(popUpUIs.Count);
        if (user != null)
        {
            popUpUIs[(int)PopupName.LogIn].SetActive(false);
            popUpUIs[(int)PopupName.Info].SetActive(false);
            Debug.Log("User is logged in.");
            // UpdateUIDText();
        }
        else
        {
            popUpUIs[(int)PopupName.LogIn].SetActive(true);
            popUpUIs[(int)PopupName.Info].SetActive(false);
            currentPopUpUI = PopupName.LogIn;
            // UpdateUIDText();
        }

        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchToTargetPopUp(PopupName targetPopUpUI)
    {
        if (currentPopUpUI != PopupName.None)
        {
            popUpUIs[(int)currentPopUpUI].SetActive(false);
        }

        if (targetPopUpUI == PopupName.None)
        {
            currentPopUpUI = PopupName.None;
            return;
        }

        currentPopUpUI = targetPopUpUI;
        popUpUIs[(int)currentPopUpUI].SetActive(true);
    }

    public void UpdateUIDText()
    {
        if (AuthManager.Instance.IsSignedIn)
            uidText.text = $"{AuthManager.Instance.UserId}";
        else
            uidText.text = "Not Signed";
    }
}
