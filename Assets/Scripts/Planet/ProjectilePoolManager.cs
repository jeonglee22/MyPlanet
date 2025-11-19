using System.Collections.Generic;
using UnityEngine;
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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("[ProjectilePoolManager] Singleton Instance set.");
        }
        else
        {
            Debug.LogWarning("[ProjectilePoolManager] Duplicate instance, destroying.");
            Destroy(gameObject);
        }
    }

    public void CreatePool(ProjectileData data)
    {
        GameObject prefab = data.projectilePrefab;
        Projectile projectile = prefab.GetComponent<Projectile>();

        Debug.Log($"[ProjectilePoolManager] CreatePool: {data.name}, " +
                  $"capacity={defaultPoolCapacity}, max={maxPoolSize}");

        objectPoolManager.CreatePool(
            data,
            data.projectilePrefab,
            defaultPoolCapacity,
            maxPoolSize,
            collectionCheck
        );
    }

    public Projectile GetProjectile(ProjectileData data)
    {
        if (!objectPoolManager.HasPool(data))
        {
            CreatePool(data);
        }

        Projectile projectile = objectPoolManager.Get(data);

        return projectile;
    }

}
