using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    private static CollectionManager instance;
    public static CollectionManager Instance => instance;

    private DatabaseReference collectionRef;

    private int towerCore;
    private int abilityCore;
    private Dictionary<int, float> weights = new Dictionary<int, float>();

    private HashSet<int> validTowerIds = new HashSet<int>();
    private HashSet<int> validAbilityIds = new HashSet<int>();

    private bool isDirty = false;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    public int TowerCore => towerCore;
    public int AbilityCore => abilityCore;

    private const int DEFAULT_TOWER_CORE = 12;
    private const int DEFAULT_ABILITY_CORE = 10;
    private const int MAX_WEIGHT = 100;

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

        InitializeValidIds();
        Debug.Log("[Collection] CollectionManager 초기화 완료");

        if(AuthManager.Instance.IsSignedIn)
        {
            InitializeReference();

            var dataSnapshot = await collectionRef.GetValueAsync().AsUniTask();

            if (dataSnapshot.Exists)
            {
                await LoadCollectionAsync();
            }
            else
            {
                towerCore = DEFAULT_TOWER_CORE;
                abilityCore = DEFAULT_ABILITY_CORE;

                isDirty = true;
                await SaveCollectionAsync();
            }
        }
        else
        {
            towerCore = DEFAULT_TOWER_CORE;
            abilityCore = DEFAULT_ABILITY_CORE;
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    private void InitializeValidIds()
    {
        validTowerIds.Clear();
        validAbilityIds.Clear();
        weights.Clear();

        var attackTowers = DataTableManager.AttackTowerTable.GetAllDatas();
        foreach (var tower in attackTowers)
        {
            int towerId = tower.AttackTower_Id;
            validTowerIds.Add(towerId);
            weights[towerId] = tower.TowerWeight;
        }

        var abilities = DataTableManager.RandomAbilityTable.GetAllAbilityIds();
        foreach (var ability in abilities)
        {
            int abilityId = ability.RandomAbility_ID;
            validAbilityIds.Add(abilityId);
            weights[abilityId] = ability.Weight;
        }
    }

    private void InitializeReference()
    {
        string userId = AuthManager.Instance.UserId;
        collectionRef = FirebaseDatabase.DefaultInstance.RootReference.Child("userdata").Child(userId).Child("collection");
    }

    public async UniTask<(bool success, string error)> LoadCollectionAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "[Collection] 로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log("[Collection] 컬렉션 로드 시도");

            DataSnapshot dataSnapshot = await collectionRef.GetValueAsync().AsUniTask();

            if (dataSnapshot.Exists)
            {
                string json = dataSnapshot.GetRawJsonValue();
                var collectionData = CollectionData.FromJson(json);

                towerCore = collectionData.towerCore;
                abilityCore = collectionData.abilityCore;

                var loadedWeights = collectionData.ToDictionary();
                foreach (var kvp in loadedWeights)
                {
                    if (validTowerIds.Contains(kvp.Key) || validAbilityIds.Contains(kvp.Key))
                    {
                        weights[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        Debug.LogWarning($"[Collection] 알 수 없는 ID 무시: {kvp.Key}");
                    }
                }

                Debug.Log("[Collection] 컬렉션 로드 성공");
            }
            else
            {
                Debug.LogWarning("[Collection] 데이터 없음, 기본값 사용");
                towerCore = DEFAULT_TOWER_CORE;
                abilityCore = DEFAULT_ABILITY_CORE;
                isDirty = true;
                return (false, "컬렉션 데이터가 존재하지 않습니다.");
            }

            isDirty = false;
            return (true, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Collection] 컬렉션 로드 실패: {e.Message}");
            return (false, e.Message);
        }
    }

    public async UniTask<(bool success, string error)> SaveCollectionAsync()
    {
        if(!AuthManager.Instance.IsSignedIn)
        {
            return (false, "[Collection] 로그인 필요");
        }

        if (!isDirty)
        {
            Debug.Log("[Collection] 변경사항 없음, 저장 생략");
            return (true, null);
        }

        try
        {
            Debug.Log("[Collection] 컬렉션 저장 시도");

            var collectionData = CollectionData.FromDictionary(towerCore, abilityCore, weights);
            string json = collectionData.ToJson();

            await collectionRef.SetRawJsonValueAsync(json).AsUniTask();

            isDirty = false;
            Debug.Log("[Collection] 컬렉션 저장 성공");

            return (true, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Collection] 컬렉션 저장 실패: {e.Message}");
            return (false, e.Message);
        }
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public void SetTowerCore(int amount)
    {
        towerCore = amount;
        MarkDirty();
    }

    public void SetAbilityCore(int amount)
    {
        abilityCore = amount;
        MarkDirty();
    }

    public bool TryUseTowerCore(int amount = 1)
    {
        if(towerCore >= amount)
        {
            towerCore -= amount;
            MarkDirty();
            return true;
        }
        return false;
    }

    public bool TryUseAbilityCore(int amount = 1)
    {
        if(abilityCore >= amount)
        {
            abilityCore -= amount;
            MarkDirty();
            return true;
        }
        return false;
    }

    public void AddTowerCore(int amount)
    {
        towerCore += amount;
        MarkDirty();
    }

    public void AddAbilityCore(int amount)
    {
        abilityCore += amount;
        MarkDirty();
    }

    public int GetWeight(int id)
    {
        return weights.TryGetValue(id, out float weight) ? (int)weight : 0;
    }

    public void SetWeight(int id, float weight)
    {
        if(!validTowerIds.Contains(id) && !validAbilityIds.Contains(id))
        {
            Debug.LogWarning($"[Collection] 알 수 없는 ID: {id}");
            return;
        }

        weights[id] = Mathf.Clamp(weight, 0, MAX_WEIGHT);
        MarkDirty();
    }

    public bool TryIncreaseWeight(int id, bool isTower)
    {
        if (isTower)
        {
            if(!TryUseTowerCore())
            {
                return false;
            }
        }
        else
        {
            if(!TryUseAbilityCore())
            {
                return false;
            }
        }

        int currentWeight = GetWeight(id);
        if(currentWeight >= MAX_WEIGHT)
        {
            if(isTower)
            {
                AddTowerCore(1);
            }
            else
            {
                AddAbilityCore(1);
            }

            return false;
        }

        SetWeight(id, currentWeight + 1);
        return true;
    }

    //public bool TryDecreaseWeight(int id, )
}
