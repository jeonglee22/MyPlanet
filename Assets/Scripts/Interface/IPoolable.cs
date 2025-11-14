using UnityEngine;

public interface IPoolable
{
    public void OnSpawnFromPool();
    public void OnReturenToPool();
}
