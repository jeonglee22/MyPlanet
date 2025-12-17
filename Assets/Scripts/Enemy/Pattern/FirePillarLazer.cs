using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirePillarLazer : MonoBehaviour
{
    private float chargeTime = 1.5f;
    private float laserLength = 10f;
    private float laserWidth = 0.5f;
    private float fieldWidth = 0.2f;
    private float expandSpeed = 10f;

    private LineRenderer lineRenderer;
    private SpriteRenderer fieldRenderer;

    private float damage;
    private float tickInterval;
    private float duration;
    private int repeatCount;
    private Vector3 direction;
    private Vector3 startPoint;
    private Vector3 endPoint;

    private Transform targetTransform;
    private int effectArrive;

    private SkillData skillData;
    private CancellationTokenSource lazerCts;

    public event Action OnLazerEnd;

    /*

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        fieldRenderer = GetComponent<SpriteRenderer>();

        fieldRenderer.sortingOrder = -1;
    }

    private void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.useWorldSpace = true;
    }

    public void Initialize(Vector3 startPosition, Vector3 direction, float damage, SkillData skillData, Action onEnd = null, float? customLength = null, CancellationToken token = default)
    {
        startPoint = startPosition;
        this.direction = direction.normalized;
        this.damage = damage;
        this.skillData = skillData;
        OnLazerEnd = onEnd;

        tickInterval = skillData.RepeatTerm;
        duration = skillData.Duration;
        repeatCount = skillData.RepeatCount;
        effectArrive = skillData.EffectArrive;

        laserLength = customLength ?? CalculateDistance();
        endPoint = startPoint + this.direction * laserLength;

        lineRenderer.enabled = false;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, startPoint);

        Cancel();

        CancellationToken linkedToken = token == default ? lazerCts.Token : CancellationTokenSource.CreateLinkedTokenSource(lazerCts.Token, token).Token;

        LazerLifeCycleAsync(linkedToken).Forget();
    }

    public void Cancel()
    {
        lazerCts?.Cancel();
        lazerCts?.Dispose();
        lazerCts = new CancellationTokenSource();
    }

    private Vector3 GetTargetPosition()
    {
        if(targetTransform != null)
        {
            return targetTransform.position;
        }

        Ge
    }
    */
}
