using UnityEngine;

public class BossCenterLocation : ISpawnLocationProvide
{
    private Transform bossTransform;
    public int bossSpawnerIndex;

    public BossCenterLocation(Transform bossTransform, int bossSpawnerIndex)
    {
        this.bossTransform = bossTransform;
        this.bossSpawnerIndex = bossSpawnerIndex;
    }

    public EnemySpawner GetSpawner()
    {
        return SpawnManager.Instance.GetSpawner(bossSpawnerIndex);
    }

    public Vector3 GetSpawnLocation()
    {
        if(bossTransform == null)
        {
            return Vector3.zero;
        }

        return bossTransform.position;
    }
}
