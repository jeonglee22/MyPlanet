using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TrapeZoidLazer : SkillBasedLazer
{
    private float startWidth = 0.3f;
    private float endWidth = 1.2f;

    protected override void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.useWorldSpace = true;

        Color changeColor = new Color(1f, 0.5f, 0f, 0.5f);
        lineRenderer.startColor = changeColor;
        lineRenderer.endColor = changeColor;
    }

    protected override void Initialize(Vector3 startPosition, Transform target, float damage, SkillData skillData, Transform owner = null, Action onEnd = null, CancellationToken token = default)
    {
        direction = Vector3.down;
        base.Initialize(startPosition, target, damage, skillData, owner, onEnd, token);
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
}
