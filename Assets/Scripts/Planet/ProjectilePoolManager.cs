using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    private static ProjectilePoolManager instance;
    public static ProjectilePoolManager Instance { get { return instance; } }

    private ObjectPoolManager<ProjectileData, Projectile> objectPoolManager = new ObjectPoolManager<ProjectileData, Projectile>();
    public ObjectPoolManager<ProjectileData, Projectile> ProjectilePool => objectPoolManager;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;

    //public GameObject ProjectilePrefab { get; private set; }
    private readonly Dictionary<ProjectileData, GameObject> prefabMap =
        new Dictionary<ProjectileData, GameObject>();

    private Dictionary<int, string> projectilePrefabName =
        new Dictionary<int, string>()
        {
            { 1100001, ObjectName.ProjectileGun },
            { 1100002, ObjectName.ProjectileGatling },
            { 1100003, ObjectName.ProjectileSniper },
            { 1100004, ObjectName.ProjectileShoot },
            { 1101001, ObjectName.ProjectileMissile },
            { 1102001, ObjectName.ProjectilePrefab },
        };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


    }

    private async UniTaskVoid OnEnable() 
    {
        await LoadPatternPrefabAsync();
    }

    private async UniTask LoadPatternPrefabAsync()
    {
        //if (ProjectilePrefab != null) return;
        //ProjectilePrefab = await Addressables.LoadAssetAsync<GameObject>(ObjectName.ProjectilePrefab).ToUniTask();
    }

    public void RegisterProjectilePrefab(ProjectileData data, GameObject prefab)
    {
        if (data == null || prefab == null) return;
        prefabMap[data] = prefab;
    }

    private GameObject GetPrefabForData(ProjectileData data)
    {
        if (data != null && prefabMap.TryGetValue(data, out var prefab) && prefab != null)
            return prefab;
        return null;
    }

    public void CreatePool(ProjectileData data)
    {
        var name = projectilePrefabName[data.Projectile_ID];

        var projectilePrefab = LoadManager.GetLoadedGamePrefab(name);
        projectilePrefab.SetActive(false);
        // GameObject ProjectilePrefab = GetPrefabForData(data);
        // Projectile projectile = projectilePrefab.GetComponent<Projectile>();

        objectPoolManager.CreatePool(
            data,
            projectilePrefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck
        );
    }

    public Projectile GetProjectile(ProjectileData data)
    {
        if (data != null && !objectPoolManager.HasPool(data))
        {
            CreatePool(data);
        }

        Projectile projectile = objectPoolManager.Get(data);
        return projectile;
    }
}