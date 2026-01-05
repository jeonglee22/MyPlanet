using System;
using UnityEngine;

public class HitScan : MonoBehaviour, System.IDisposable
{
    private ParticleSystem hitScanEffect;
    private Enemy targetEnemy;
    private float hitScanDuration = 0.2f;
    private float hitScanTimer = 0f;

    private void Awake()
    {
        hitScanEffect = GetComponentInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        hitScanTimer = 0f;
    }

    void Update()
    {

        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy || targetEnemy.IsDead)
        {
            Despawn();
            return;
        }

        transform.position = targetEnemy.transform.position;
        hitScanTimer += Time.deltaTime;
        if(hitScanTimer >= hitScanDuration)
        {
            Despawn();
            return;
        }
    }

    public void SetHitScan(Enemy enemy, float timer)
    {
        targetEnemy = enemy;
        hitScanDuration = timer;
        transform.position = targetEnemy.transform.position;

        if (targetEnemy != null)
            transform.position = targetEnemy.transform.position;

        if (hitScanEffect != null)
        {
            hitScanEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitScanEffect.Play(true);
        }
    }

    private void Despawn()
    {
        targetEnemy = null;
        hitScanTimer = 0f;

        if (hitScanEffect != null)
            hitScanEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (HitScanPoolManager.Instance != null)
            HitScanPoolManager.Instance.Return(this);
        else
            gameObject.SetActive(false);
    }


    public void Dispose()
    {
        targetEnemy = null;
        hitScanTimer = 0f;

        if (hitScanEffect != null)
            hitScanEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}