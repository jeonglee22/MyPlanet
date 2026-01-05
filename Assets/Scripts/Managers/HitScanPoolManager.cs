using UnityEngine;

public class HitScanPoolManager : MonoBehaviour
{
    public static HitScanPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 50;
    [SerializeField] private int maxPoolSize = 500;

    [Header("Optional Parent")]
    [SerializeField] private Transform poolParent;

    private static readonly string HITSCAN_KEY = ObjectName.HitScan;

    private readonly ObjectPoolManager<string, HitScan> pool
        = new ObjectPoolManager<string, HitScan>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        EnsurePool();
    }

    private void EnsurePool()
    {
        if (pool.HasPool(HITSCAN_KEY)) return;

        var prefab = LoadManager.GetLoadedGamePrefabOriginal(HITSCAN_KEY);
        if (prefab == null)
        {
            Debug.LogError($"[HitScanPoolManager] Prefab not loaded: {HITSCAN_KEY}. LoadManager에서 라벨 로드가 먼저 되어야 합니다.");
            return;
        }

        pool.CreatePool(
            HITSCAN_KEY,
            prefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck,
            poolParent
        );
    }

    public HitScan Get()
    {
        EnsurePool();

        if (!pool.HasPool(HITSCAN_KEY))
        {
            Debug.LogError($"[HitScanPoolManager] Pool not ready: {HITSCAN_KEY}");
            return null;
        }

        return pool.Get(HITSCAN_KEY);
    }


    public void Return(HitScan hitScan)
    {
        if (hitScan == null) return;
        pool.Return(HITSCAN_KEY, hitScan);
    }
}
