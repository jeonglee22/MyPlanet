using System;
using UnityEngine;
using UnityEngine.VFX;

public class PooledFx : MonoBehaviour, IDisposable
{
    [Header("Auto Despawn")]
    [SerializeField] private bool autoDespawn = true;
    [Tooltip("0보다 크면 이 시간(초) 뒤에 무조건 Despawn. 0이면 파티클 duration 기반으로 자동 계산")]
    [SerializeField] private float lifeOverride = 0f;

    private ParticleSystem[] particles;
    private float timer;
    private float life;

    // FxManager가 세팅해줌
    public FxId PoolKey { get; set; }
    public FxManager Owner { get; set; }

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        timer = 0f;

        if (particles != null)
        {
            foreach (var p in particles)
            {
                if (p == null) continue;
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                p.Play(true);
            }
        }

        life = (lifeOverride > 0f) ? lifeOverride : EstimateLifeSeconds();
    }

    private void Update()
    {
        if (!autoDespawn) return;

        if (lifeOverride > 0f)
        {
            timer += Time.deltaTime;
            if (timer >= lifeOverride) Despawn();
            return;
        }

        // 파티클이 하나라도 살아있으면 유지
        if (particles != null)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                var p = particles[i];
                if (p != null && p.IsAlive(true)) return;
            }
        }

        Despawn();
    }


    private float EstimateLifeSeconds()
    {
        float max = 0.2f;
        if (particles == null || particles.Length == 0) return max;

        foreach (var p in particles)
        {
            if (p == null) continue;

            var main = p.main;
            float lt =
                main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                    ? main.startLifetime.constantMax
                    : main.startLifetime.constant;

            max = Mathf.Max(max, main.duration + lt);
        }

        return max + 0.05f;
    }

    public void Despawn()
    {
        if (Owner != null) Owner.Return(this);
        else gameObject.SetActive(false);
    }

    public void Dispose()
    {
        timer = 0f;

        if (particles != null)
        {
            foreach (var p in particles)
            {
                if (p == null) continue;
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
