using UnityEngine;

public class LazerPattern : ShootingPattern
{
    public override int PatternId => patternData.Pattern_Id;

    private Transform target;
    private GameObject lazerObject;

    private float duration = 2f;
    private float laserWidth = 0.2f;
    private float tickInterval = 0.1f;


    public override void Initialize(Enemy enemy, EnemyMovement movement, EnemyTableData enemyData)
    {
        base.Initialize(enemy, movement, enemyData);

        Trigger = ExecutionTrigger.OnInterval;

        var player = GameObject.FindGameObjectWithTag(TagName.Planet);
        if (player != null)
        {
            target = player.transform;
        }
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
        lazer.Initialize(shootPosition, shootDirection, damage);

        lazerObject.SetActive(true);
    }

    protected virtual Vector3 GetLaserDirection()
    {
        return Vector3.down;
    }

    private void LoadLaserSettings()
    {
        lazerObject = LoadManager.GetLoadedGamePrefab(AddressLabel.Lazer);

        duration = 2f;
        laserWidth = 0.2f;
        tickInterval = 0.1f;
    }
}
