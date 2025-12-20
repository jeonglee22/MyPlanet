using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TrapeZoidLazer : SkillBasedLazer
{
    private float startWidth = 0.3f;
    private float endWidth = 1.2f;

    private int heightSamples = 10;
    private int widthRays = 5;

    protected override void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.useWorldSpace = true;
    }

    public override void Initialize(Vector3 startPosition, Transform target, float damage, SkillData skillData, Transform owner = null, Action onEnd = null, CancellationToken token = default)
    {
        direction = Vector3.down;
        base.Initialize(startPosition, target, damage, skillData, owner, onEnd, token);
    }

    protected override void UpdateLaserWidth(float currentLength)
    {
        float progress = currentLength / laserLength;
        float currentEndWidth = Mathf.Lerp(startWidth, endWidth, progress);

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = currentEndWidth;
    }

    protected override void SetupParticle()
    {
        if(laserParticle != null)
        {
            laserParticle.gameObject.SetActive(true);
            laserParticle.gameObject.transform.position = startPoint;
            laserParticle.gameObject.transform.rotation = Quaternion.identity;
            laserParticle.transform.localScale = Vector3.one;

            var main = laserParticle.main;
            main.startSpeed = 0.1f;

            var shape = laserParticle.shape;
            shape.scale = new Vector3(0f, 0f, 2f);

            laserParticle.Clear();
            laserParticle.Play();
        }
    }

    protected override bool CheckLazerCollision(float currentLength)
    {
        bool hited = false;
        HashSet<IDamagable> damagables = new HashSet<IDamagable>();

        for(int i = 0; i < heightSamples; i++)
        {
            float heightRatio = i / (float)(heightSamples - 1);

            float checkHeight = currentLength * heightRatio;

            float widthHeight = Mathf.Lerp(startWidth, endWidth, heightRatio);

            Vector3 heightStartPoint = startPoint + direction * checkHeight;

            float remainingLength = currentLength - checkHeight;
            if(remainingLength <= 0f)
            {
                continue;
            }

            for(int j = 0; j < widthRays; j++)
            {
                float widthRatio = j / (float)(widthRays - 1) * 2f - 1f;
                
                Vector3 offSet = Vector3.right * (widthRatio * widthHeight / 2f);
                Vector3 rayOrigin = heightStartPoint + offSet;

                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, direction, remainingLength);

                foreach(RaycastHit hit in hits)
                {
                    if(hit.collider.CompareTag(TagName.Enemy) || 
                    hit.collider.CompareTag(TagName.Boss) ||
                    hit.collider.CompareTag(TagName.CenterStone) ||
                    hit.collider.CompareTag(TagName.Projectile) ||
                    hit.collider.CompareTag(TagName.PatternLine))
                    {
                        continue;
                    }

                    IDamagable damagable = hit.collider.GetComponent<IDamagable>();
                    if(damagable != null && !damagables.Contains(damagable))
                    {
                        damagable.OnDamage(damage);
                        damagables.Add(damagable);
                        hited = true;
                    }
                }
            }
        }

        return hited;
    }

    protected override async UniTask ExpandingAttackPhaseAsync(CancellationToken token)
    {
        fieldRenderer.enabled = false;
        lineRenderer.enabled = true;

        transform.position = startPoint;
        transform.rotation = Quaternion.identity;

        if(laserParticle != null)
        {
            laserParticle.gameObject.SetActive(true);
            laserParticle.gameObject.transform.position = startPoint;
            laserParticle.gameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            laserParticle.transform.localScale = Vector3.one;

            var main = laserParticle.main;
            main.startSpeed = 0.1f;

            var shape = laserParticle.shape;
            shape.position = new Vector3(0f, 0f, 2f);

            laserParticle.Clear();
            laserParticle.Play();
        }

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, startPoint);

        float currentLength = 0f;
        float tickTimer = tickInterval;
        int currentTickCount = 0;

        while(currentLength < laserLength)
        {
            token.ThrowIfCancellationRequested();

            tickTimer += Time.deltaTime;

            currentLength += expandSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength, laserLength);

            Vector3 currentEndPoint = startPoint + direction * currentLength;
            lineRenderer.SetPosition(1, currentEndPoint);

            UpdateLaserWidth(currentLength);

            if(laserParticle != null)
            {
                var main = laserParticle.main;
                main.startSpeed = currentLength;
            }

            if(tickTimer >= tickInterval && currentTickCount < repeatCount)
            {
                bool hitSomething = CheckLazerCollision(currentLength);
                if (hitSomething)
                {
                    currentTickCount++;
                }
                tickTimer = 0f;
            }

            await UniTask.Yield(token);
        }

        tickTimer = tickInterval;

        float expandTime = laserLength / expandSpeed;
        float remainingTime = Mathf.Max(0f, duration - expandTime);
        float elapsedTime = 0f;

        while(elapsedTime < remainingTime)
        {
            token.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if(tickTimer >= tickInterval && currentTickCount < repeatCount)
            {
                bool hitSomething = CheckLazerCollision(laserLength);
                if (hitSomething)
                {
                    currentTickCount++;
                }
                tickTimer = 0f;
            }

            await UniTask.Yield(token);
        }

        lineRenderer.enabled = false;

        if(laserParticle != null)
        {
            laserParticle.Stop();
            laserParticle.Clear();
        }
    }
}
