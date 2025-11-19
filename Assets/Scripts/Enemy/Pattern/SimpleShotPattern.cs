using UnityEngine;

public class SimpleShotPattern : ShootingPattern
{
    private float shootSpeed = 3f;

    protected override void Shoot()
    {
        Vector3 spawnPosition = owner.transform.position;
        Vector3 shootDirection = Vector3.down;

        spawner.SpawnPattern(spawnPosition, shootDirection, owner.atk, shootSpeed, owner.LifeTime);
    }
}
