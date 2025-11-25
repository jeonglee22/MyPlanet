using UnityEngine;

public interface ISpawnLocationProvide
{
    public Vector3 GetSpawnLocation();
    public EnemySpawner GetSpawner();
}
