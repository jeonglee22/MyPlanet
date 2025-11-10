using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    private static ProjectilePoolManager instance;
    public static ProjectilePoolManager Instance { get { return instance; } }

    private Dictionary<ProjectileData, IObjectPool<Projectile>> projectilePools = new Dictionary<ProjectileData, IObjectPool<Projectile>>();
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultPoolCapacity = 20;
    [SerializeField] private int maxPoolSize = 100;

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

    public void CreatePool(ProjectileData data)
    {
        GameObject prefab = data.projectilePrefab;
        Projectile projectile = prefab.GetComponent<Projectile>();

        IObjectPool<Projectile> pool = new ObjectPool<Projectile>(
            () => CreateProjectile(data, projectilePools[data]),
            (obj) => obj.gameObject.SetActive(true),
            (obj) => obj.gameObject.SetActive(false),
            (obj) => Destroy(obj.gameObject),
            collectionCheck,
            defaultPoolCapacity,
            maxPoolSize
        );

        projectilePools.Add(data, pool);
    }

    public Projectile GetProjectile(ProjectileData data)
    {
        if (!projectilePools.ContainsKey(data))
        {
            CreatePool(data);
        }

        return projectilePools[data].Get();
    }
    
    private Projectile CreateProjectile(ProjectileData data, IObjectPool<Projectile> pool)
    {
        GameObject projectileObj = Instantiate(data.projectilePrefab);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.SetPool(pool);
        return projectile;
    }

}
