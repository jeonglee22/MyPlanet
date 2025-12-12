using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private static CurrencyManager instance;
    public static CurrencyManager Instance => instance;

    private DatabaseReference currencyRef;

    private UserCurrencyData userCurrencyData;
    private bool isDirty = false;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    public int CachedGold => userCurrencyData?.gold ?? 0;
    public int CachedFreeDia => userCurrencyData?.freeDia ?? 0;
    public int CachedChargedDia => userCurrencyData?.chargedDia ?? 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid Start()
    {
        await FireBaseInitializer.Instance.WaitInitialization();

        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        if (AuthManager.Instance.IsSignedIn)
        {
            InitializeReference();
            var dataSnapshot = await currencyRef.GetValueAsync().AsUniTask();

            if(dataSnapshot.Exists)
            {
                await LoadCurrencyAsync();
            }
            else
            {
                if(userCurrencyData == null)
                {
                    userCurrencyData = new UserCurrencyData();
                }

                isDirty = true;
                await SaveCurrencyAsync();
            }
        }

        isInitialized = true;
    }

    private void InitializeReference()
    {
        string userId = AuthManager.Instance.UserId;
        currencyRef = FirebaseDatabase.DefaultInstance.RootReference.Child("userdata").Child(userId).Child("currency");
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public async UniTask<(bool success, string error)> LoadCurrencyAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "[Currency] 로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log("[Currency] 화페 로드 시도");

            DataSnapshot dataSnapshot = await currencyRef.GetValueAsync().AsUniTask();

            if (dataSnapshot.Exists)
            {
                string json = dataSnapshot.GetRawJsonValue();
                userCurrencyData = UserCurrencyData.FromJson(json);

                Debug.Log("[Currency] 화페 로드 성공");
            }
            else
            {
                Debug.Log("[Currency] 화페 데이터 없음, 새로 생성");

                isDirty = true;
                await SaveCurrencyAsync();
            }

            isDirty = false;
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Currency] 화페 로드 실패: {ex.Message}");

            return (false, ex.Message);
        }
    }

    public async UniTask<(bool success, string error)> SaveCurrencyAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "[Currency] 로그인 필요");
        }

        if (!isDirty)
        {
            Debug.Log("[Currency] 변경사항 없음, 저장 생략");
            return (true, null);
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log("[Currency] 화페 저장 시도");

            string json = userCurrencyData.ToJson();
            await currencyRef.SetRawJsonValueAsync(json).AsUniTask();

            isDirty = false;
            Debug.Log("[Currency] 화페 저장 성공");

            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Currency] 화페 저장 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public void SetGold(int value)
    {
        if(userCurrencyData == null)
        {
            userCurrencyData = new UserCurrencyData();
        }

        userCurrencyData.gold = value;
        MarkDirty();
    }

    public void SetFreeDia(int value)
    {
        if(userCurrencyData == null)
        {
            userCurrencyData = new UserCurrencyData();
        }

        userCurrencyData.freeDia = value;
        MarkDirty();
    }

    public void SetChargedDia(int value)
    {
        if(userCurrencyData == null)
        {
            userCurrencyData = new UserCurrencyData();
        }

        userCurrencyData.chargedDia = value;
        MarkDirty();
    }
}
