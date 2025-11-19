using UnityEngine;

public class SimpleShotPattern : ShootingPattern
{
    [SerializeField] private int patternId = 0;
    private float shootSpeed = 3f;
    private float projectileLifeTime = 3f;

    protected override void Shoot()
    {
        Vector3 spawnPosition = owner.transform.position;
        Vector3 shootDirection = Vector3.down;

        spawner.SpawnPattern(spawnPosition, shootDirection, owner.atk, shootSpeed, projectileLifeTime);
    }
}
