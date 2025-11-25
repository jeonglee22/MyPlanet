using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnerLocation : ISpawnLocationProvide
{
    private List<int> allowedSpawners;
    private int lastSelectedIndex = -1;

    public RandomSpawnerLocation(List<int> allowedSpawners)
    {
        this.allowedSpawners = new List<int>(allowedSpawners);
    }

    public EnemySpawner GetSpawner()
    {
        if(allowedSpawners == null || allowedSpawners.Count == 0)
        {
            return null;
        }

        lastSelectedIndex = allowedSpawners[Random.Range(0, allowedSpawners.Count)];
        return SpawnManager.Instance.GetSpawner(lastSelectedIndex);
    }

    public Vector3 GetSpawnLocation()
    {
        EnemySpawner spawner = GetSpawner();
        if(spawner == null)
        {
            return Vector3.zero;
        }

        return spawner.transform.position;
    }
}
