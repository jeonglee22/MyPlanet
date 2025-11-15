using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;


public class ObjectPoolManager<TKey, TValue> where TValue : Component
{
    private struct PoolData
    {
        public ObjectPool<TValue> pool;
        public GameObject prefab;
        public Transform parent;
        public int maxSize;
        public bool collectionCheck;

        public PoolData(ObjectPool<TValue> pool, GameObject prefab, Transform parent, int maxSize, bool collectionCheck)
        {
            this.pool = pool;
            this.prefab = prefab;
            this.parent = parent;
            this.maxSize = maxSize;
            this.collectionCheck = collectionCheck;
        }
    }

    private readonly Dictionary<TKey, PoolData> pools = new Dictionary<TKey, PoolData>();

    public void CreatePool(TKey key, GameObject prefab, int initialSize = 50, int maxSize = 100, bool collectionCheck = true, Transform parent = null)
    {
        if (HasPool(key) || prefab == null)
        {
            return;
        }

        ObjectPool<TValue> pool = new ObjectPool<TValue>(
            () => CreateInstance(prefab, parent),
            (instance) => OnGet(instance),
            (instance) => OnRelease(instance, parent),
            (instance) => OnDestroy(instance),
            collectionCheck,
            initialSize,
            maxSize
        );

        PoolData poolData = new PoolData(pool, prefab, parent, maxSize, collectionCheck);
        pools.Add(key, poolData);

        Preload(key, initialSize);
    }

    public TValue Get(TKey key)
    {
        if (!pools.TryGetValue(key, out PoolData poolData))
        {
            return null;
        }

        return poolData.pool.Get();
    }

    public void Return(TKey key, TValue value)
    {
        if (!pools.TryGetValue(key, out PoolData poolData))
        {
            Object.Destroy(value.gameObject);
            return;
        }

        poolData.pool.Release(value);
    }

    public void Preload(TKey key, int count)
    {
        if (!pools.ContainsKey(key))
        {
            return;
        }

        List<TValue> tempList = new List<TValue>(count);
        for (int i = 0; i < count; i++)
        {
            TValue instance = Get(key);
            if (instance != null)
            {
                tempList.Add(instance);
            }
        }

        foreach (var item in tempList)
        {
            Return(key, item);
        }

        tempList.Clear();
    }

    public bool HasPool(TKey key) => pools.ContainsKey(key);

    public void DestroyPool(TKey key)
    {
        if (!pools.TryGetValue(key, out PoolData poolData))
        {
            return;
        }

        poolData.pool.Clear();

        if (poolData.parent != null)
        {
            Object.Destroy(poolData.parent.gameObject);
        }

        pools.Remove(key);
    }

    public void Clear()
    {
        List<TKey> keys = new List<TKey>(pools.Keys);

        foreach (var key in keys)
        {
            DestroyPool(key);
        }

        pools.Clear();
    }

    //Call backs
    private TValue CreateInstance(GameObject prefab, Transform parent)
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        TValue component = obj.GetComponent<TValue>();

        if (component == null)
        {
            Object.Destroy(obj);
            return null;
        }

        obj.SetActive(false);
        return component;
    }

    private void OnGet(TValue instance)
    {
        instance.gameObject.SetActive(true);
    }

    private void OnRelease(TValue instance, Transform parent)
    {
        instance.gameObject.SetActive(false);

        instance.transform.SetParent(parent);
    }
    
    private void OnDestroy(TValue instance)
    {
        Object.Destroy(instance.gameObject);
    }
}
