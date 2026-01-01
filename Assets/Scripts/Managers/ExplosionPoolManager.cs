using UnityEngine;

public class ExplosionPoolManager : MonoBehaviour
{
    public static ExplosionPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 50;
    [SerializeField] private int maxPoolSize = 500;

    [Header("Optional Parent")]
    [SerializeField] private Transform poolParent;

    private static readonly string EXPLOSION_KEY = ObjectName.Explosion;

    private readonly ObjectPoolManager<string, Explosion> pool
        = new ObjectPoolManager<string, Explosion>();

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
        if (pool.HasPool(EXPLOSION_KEY)) return;

        var prefab = LoadManager.GetLoadedGamePrefabOriginal(EXPLOSION_KEY);
        if (prefab == null)
        {
            Debug.LogError($"[ExplosionPoolManager] Prefab not loaded: {EXPLOSION_KEY}. LoadManager 라벨 로드 선행 필요");
            return;
        }

        pool.CreatePool(
            EXPLOSION_KEY,
            prefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck,
            poolParent
        );
    }

    public Explosion Get()
    {
        EnsurePool();
        return pool.Get(EXPLOSION_KEY);
    }

    public void Return(Explosion explosion)
    {
        if (explosion == null) return;
        pool.Return(EXPLOSION_KEY, explosion);
    }
}
