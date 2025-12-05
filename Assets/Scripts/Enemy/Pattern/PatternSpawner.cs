using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PatternSpawner : MonoBehaviour
{
    public static PatternSpawner Instance { get; private set; }

    private ObjectPoolManager<int, PatternProjectile> objectPoolManager = new ObjectPoolManager<int, PatternProjectile>();

    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 500;

    private bool isLoaded = false;
    private GameObject patternPrefab;
    private const int patternProjectileId = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTaskVoid OnEnable() 
    {
        await LoadPatternPrefabAsync();
        PreparePool();
    }

    private async UniTask LoadPatternPrefabAsync()
    {
        if(patternPrefab != null)
        {
            return;
        }

        patternPrefab = await Addressables.LoadAssetAsync<GameObject>(ObjectName.PatternProjectile).ToUniTask();
    }

    private void PreparePool()
    {
        if(objectPoolManager.HasPool(patternProjectileId) || patternPrefab == null)
        {
            return;
        }

        objectPoolManager.CreatePool
        (
            patternProjectileId,
            patternPrefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck,
            this.transform
        );

        isLoaded = true;
    }

    public PatternProjectile SpawnPattern(Vector3 position, Vector3 direction, float damage, float speed, float lifeTime)
    {
        if(!isLoaded)
        {
            return null;
        }

        PatternProjectile pattern = objectPoolManager.Get(patternProjectileId);
        if(pattern == null)
        {
            return null;
        }

        pattern.transform.position = position;
        pattern.transform.rotation = Quaternion.LookRotation(direction);

        pattern.Initialize(patternProjectileId, damage, speed, lifeTime, direction, this);

        return pattern;
    }

    public void ReturnPatternToPool(PatternProjectile pattern)
    {
        if(pattern == null)
        {
            return;
        }

        objectPoolManager.Return(patternProjectileId, pattern);
    }
}
