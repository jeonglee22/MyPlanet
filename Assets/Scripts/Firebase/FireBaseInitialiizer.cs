using UnityEngine;
using Firebase;
using Cysharp.Threading.Tasks;
using System;

public class FireBaseInitializer : MonoBehaviour
{
    private static FireBaseInitializer instance;
    public static FireBaseInitializer Instance => instance;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private FirebaseApp firebaseApp;
    public FirebaseApp FirebaseApp => firebaseApp;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase().Forget();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private async UniTaskVoid InitializeFirebase()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();

            if (dependencyStatus == DependencyStatus.Available)
            {
                firebaseApp = FirebaseApp.DefaultInstance;
                isInitialized = true;
                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                isInitialized = false;
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }
        catch (Exception ex)
        {
            isInitialized = false;
            Debug.LogError($"Firebase initialization failed. Exception: {ex.Message}");
        }
    }

    public async UniTask WaitInitialization()
    {
        await UniTask.WaitUntil(() => isInitialized);
    }
}
