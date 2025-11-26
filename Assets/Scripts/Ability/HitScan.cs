using UnityEngine;

public class HitScan : MonoBehaviour
{
    private ParticleSystem hitScanEffect;
    private Enemy targetEnemy;
    private float hitScanDuration = 0.2f;
    private float hitScanTimer = 0f;

    private void Awake()
    {
        hitScanEffect = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        transform.position = targetEnemy.transform.position;
        hitScanTimer += Time.deltaTime;
        if(hitScanTimer >= hitScanDuration)
        {
            Destroy(gameObject);
        }
    }

    public void SetHitScan(Enemy enemy, float timer)
    {
        targetEnemy = enemy;
        hitScanDuration = timer;
        transform.position = targetEnemy.transform.position;
    }
}
