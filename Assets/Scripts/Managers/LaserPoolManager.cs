using UnityEngine;

public class LaserPoolManager : MonoBehaviour
{
    public static LaserPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 200;

    [Header("Optional Parent")]
    [SerializeField] private Transform poolParent;

    private static readonly string LASER_KEY = ObjectName.Lazer;

    private readonly ObjectPoolManager<string, LazertowerAttack> pool
        = new ObjectPoolManager<string, LazertowerAttack>();

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
        if (pool.HasPool(LASER_KEY)) return;

        // IMPORTANT: 원본 프리팹을 가져와야 함(Instantiate 금지)
        var prefab = LoadManager.GetLoadedGamePrefabOriginal(LASER_KEY);
        if (prefab == null)
        {
            Debug.LogError($"[LaserPoolManager] Prefab not loaded: {LASER_KEY}. LoadManager에서 라벨 로드가 먼저 되어야 합니다.");
            return;
        }

        pool.CreatePool(
            LASER_KEY,
            prefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck,
            poolParent
        );
    }

    public LazertowerAttack GetLaser()
    {
        Debug.Log("[LASER][POOL] GetLaser ENTER");

        EnsurePool();
        return pool.Get(LASER_KEY);
    }

    public void ReturnLaser(LazertowerAttack laser)
    {
        if (laser == null) return;
        pool.Return(LASER_KEY, laser);
    }
}
