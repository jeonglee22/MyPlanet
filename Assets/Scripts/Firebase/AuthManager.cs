using System;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Google;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;
    public static AuthManager Instance => instance;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    public FirebaseUser CurrentUser => currentUser;
    public string UserId => currentUser != null ? currentUser.UserId : string.Empty;
    public string UserEmail => currentUser != null ? currentUser.Email : string.Empty;
    public bool IsSignedIn => currentUser != null;
    private string nickName = string.Empty;
    private bool isGoogleSignIn = false;

    public string UserNickName => currentUser != null ? nickName : string.Empty;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = "825756151038-nmmg915h2euugdnjnmi4n5f1k4a2l03u.apps.googleusercontent.com",
            RequestEmail = true,
            RequestIdToken = true,
            UseGameSignIn = false,
        };
    }

    private async UniTaskVoid Start()
    {
        await FireBaseInitializer.Instance.WaitInitialization();

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;

        currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            var userRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserPlanets);
            var dataSnapshot = await userRef.Child(UserId).GetValueAsync().AsUniTask();

            if (dataSnapshot.Exists)
            {
                nickName = dataSnapshot.Child("nickName").Value.ToString();
            }
        }

        isInitialized = true;
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs args)
    {
        if (auth.CurrentUser != currentUser) {
            bool signedIn = currentUser != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && currentUser != null) 
            {
                Debug.Log("Signed out " + currentUser.UserId);
            }

            currentUser = auth.CurrentUser;

            if (signedIn) 
            {
                Debug.Log("Signed in " + currentUser.UserId);
            }
        }
    }

    public async UniTask<bool> SignInAnonymousAsync(string nickName = "")
    {
        try
        {
            var authResult = await auth.SignInAnonymouslyAsync().AsUniTask();
            currentUser = authResult.User;
            this.nickName = nickName;

            await PlanetManager.Instance.ReloadPlanetsForNewUser();

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"Signed in Anonymous Error: {ex.Message}");
            return false;
        }
    }

    public async UniTask<(bool, string)> SignInWithGoogleAsync()
    {
        GoogleSignInUser signResult = null;

        try
        {
            signResult = await GoogleSignIn.DefaultInstance.SignIn().AsUniTask();
        }
        catch (Exception ex)
        {
            Debug.Log($"Google Sign-In Error: {ex.Message}");
            return (false, ex.Message);   
        }

        try
        {
            Credential credential = GoogleAuthProvider.GetCredential(signResult.IdToken, null);
            var authResult = await auth.SignInWithCredentialAsync(credential).AsUniTask();
            currentUser = authResult;

            await PlanetManager.Instance.ReloadPlanetsForNewUser();

            isGoogleSignIn = true;

            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.Log($"Signed in with Google Error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<bool> CreateAccountWithEmailAsync(string email, string password, string nickName)
    {
        try
        {
            var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
            currentUser = authResult.User;
            this.nickName = nickName;

            await PlanetManager.Instance.ReloadPlanetsForNewUser();

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"Signed in Anonymous Error: {ex.Message}");
            return false;
        }
    }

    public async UniTask<bool> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            var authResult = await auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
            currentUser = authResult.User;

            var userRef = FirebaseDatabase.DefaultInstance.GetReference(DatabaseRef.UserPlanets);
            var dataSnapshot = await userRef.Child(UserId).GetValueAsync().AsUniTask();
            if (dataSnapshot.Exists)
            {
                nickName = dataSnapshot.Child("nickName").Value.ToString();
            }

            await PlanetManager.Instance.ReloadPlanetsForNewUser();

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"Signed in Anonymous Error: {ex.Message}");
            return false;
        }
    }

    public void SignOut()
    {
        if(auth != null && currentUser != null)
        {
            auth.SignOut();
            currentUser = null;
            nickName = string.Empty;
            Variables.Reset();
            Variables.planetId = 300001;
            UserAttackPowerManager.Instance.PlanetPower = 0f;
            UserAttackPowerManager.Instance.TowerPower = 0f;
            UserAttackPowerManager.Instance.SimilarAttackPowerUserId = string.Empty;

            PlanetManager.Instance?.ClearPlanetData();

            if (isGoogleSignIn)
            {
                GoogleSignIn.DefaultInstance.SignOut();
                isGoogleSignIn = false;
            }
        }
    }
}