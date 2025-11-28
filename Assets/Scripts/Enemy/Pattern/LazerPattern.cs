using UnityEngine;

public class LazerPattern : ShootingPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private GameObject lazerObject;

    protected float duration = 2f;
    protected float laserWidth = 1f;
    protected float tickInterval = 0.1f;


    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);

        Trigger = ExecutionTrigger.OnInterval;
    }

    protected override void Shoot()
    {
        if(lazerObject == null)
        {
            LoadLaserSettings();
        }

        Vector3 shootPosition = owner.transform.position;
        Vector3 shootDirection = GetLaserDirection();

        Lazer lazer = lazerObject.GetComponent<Lazer>();

        lazer.SetDuration(duration);
        lazer.SetLazerWidth(laserWidth);
        lazer.SetTickInterval(tickInterval);

        float damage = owner.atk;

        if(movement != null)
        {
            movement.CanMove = false;
        }

        lazer.Initialize(shootPosition, shootDirection, damage, OnLazerComplete);

        lazerObject.SetActive(true);
    }

    protected virtual Vector3 GetLaserDirection()
    {
        return Vector3.down;
    }

    private void LoadLaserSettings()
    {
        lazerObject = LoadManager.GetLoadedGamePrefab(AddressLabel.EnemyLazer);

        duration = 2f;
        laserWidth = 0.2f;
        tickInterval = 0.1f;
    }

    private void OnLazerComplete()
    {
        if(movement != null)
        {
            movement.CanMove = true;
        }
    }
}
