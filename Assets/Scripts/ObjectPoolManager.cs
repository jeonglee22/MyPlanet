using UnityEngine;
using UnityEngine.Pool;


public class ObjectPoolManager<TKey, TValue> where TValue : Component
{
    private struct PoolData
    {
        public ObjectPool<TValue> pool;
    }
}
