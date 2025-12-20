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
    private float expandSpeed = 5f;

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

    private ParticleSystem laserParticle;

    private Transform ownerTransform;
    private float particleOffsetY;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        fieldRenderer = GetComponent<SpriteRenderer>();
        laserParticle = GetComponentInChildren<ParticleSystem>();

        fieldRenderer.sortingOrder = -1;
    }

    private void OnEnable()
    {
        Setup();
    }

    private void OnDisable()
    {
        Cancel();

        if(laserParticle != null)
        {
            laserParticle.Stop();
            laserParticle.Clear();
        }
    }

    private void OnDestroy()
    {
        Cancel();
    }

    private void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.useWorldSpace = true;

        Color transparentColor = new Color(1f, 1f, 1f, 0f);
        lineRenderer.startColor = transparentColor;
        lineRenderer.endColor = transparentColor;
    }

    public void Initialize(Vector3 startPosition, Transform target, float damage, SkillData skillData, Transform owner = null, Action onEnd = null, CancellationToken token = default)
    {
        startPoint = startPosition;
        targetTransform = target;
        this.damage = damage;
        this.skillData = skillData;
        OnLazerEnd = onEnd;

        tickInterval = skillData.RepeatTerm;
        duration = skillData.Duration;
        repeatCount = skillData.RepeatCount;
        effectArrive = skillData.EffectArrive;

        ownerTransform = owner;
        if(ownerTransform != null)
        {
            SphereCollider ownerCollider = ownerTransform.GetComponent<SphereCollider>();
            if(ownerCollider != null)
            {
                particleOffsetY = ownerCollider.radius;
            }
        }

        fieldRenderer.enabled = false;
        lineRenderer.enabled = false;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, startPoint);

        SetupFinalField();

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

    private async UniTaskVoid LazerLifeCycleAsync(CancellationToken token)
    {
        try
        {
            await ChargePhaseWithTrackAsync(token);

            await ExpandingAttackPhaseAsync(token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if(laserParticle != null)
            {
                laserParticle.Stop();
                laserParticle.Clear();
            }

            OnLazerEnd?.Invoke();
            OnLazerEnd = null;

            if(this != null && gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private async UniTask ChargePhaseWithTrackAsync(CancellationToken token)
    {
        float elapsedTime = 0f;
        fieldRenderer.enabled = true;

        if(laserParticle != null)
        {
            laserParticle.gameObject.SetActive(false);
        }

        while (elapsedTime < chargeTime)
        {
            token.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;

            Vector3 targetPosition = GetTargetPosition();
            direction = (targetPosition - startPoint).normalized;

            float distanceTarget = Vector3.Distance(startPoint, targetPosition);

            transform.position = startPoint;

            if(direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }

            fieldRenderer.transform.localScale = new Vector3(fieldWidth, distanceTarget, 1f);
            
            Vector3 fieldCenter = startPoint + direction * (distanceTarget * 0.5f);
            fieldRenderer.transform.position = fieldCenter;
            fieldRenderer.transform.rotation = transform.rotation;

            if(laserParticle != null)
            {
                laserParticle.transform.position = startPoint;
                laserParticle.transform.rotation = transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
            }

            await UniTask.Yield(token);
        }

        Vector3 finalTargetPosition = GetTargetPosition();
        direction = (finalTargetPosition - startPoint).normalized;

        Rect screenBounds = SpawnManager.Instance.ScreenBounds;
        float screenBottomY = screenBounds.yMin;
        float distanceBottom = startPoint.y - screenBottomY;
        laserLength = Mathf.Max(0.1f, distanceBottom);

        endPoint = startPoint + direction * laserLength;

        SetupFinalField();
    }

    private Vector3 GetTargetPosition()
    {
        if(targetTransform != null)
        {
            return targetTransform.position;
        }

        GameObject planet = GameObject.FindGameObjectWithTag(TagName.Planet);
        if(planet != null)
        {
            return planet.transform.position;
        }

        return startPoint + Vector3.down * 10f;
    }

    private void SetupFinalField()
    {
        transform.position = startPoint;

        if(direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }

        fieldRenderer.transform.localScale = new Vector3(fieldWidth, laserLength, 1f);
        fieldRenderer.transform.localPosition = new Vector3(0f, laserLength * 0.5f, 0f);
    }

    public void SetDuration(float duration) => this.duration = duration;
    public void SetTickInterval(float interval) => tickInterval = interval;
    public void SetLazerWidth(float width) => laserWidth = width;

    private async UniTask ExpandingAttackPhaseAsync(CancellationToken token)
    {
        fieldRenderer.enabled = false;
        lineRenderer.enabled = true;

        transform.position = startPoint;

        if(laserParticle != null)
        {
            laserParticle.gameObject.SetActive(true);

            laserParticle.gameObject.transform.position = startPoint - direction * particleOffsetY;
            laserParticle.gameObject.transform.rotation = transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
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
                bool hitSomthing = CheckLazerCollision(laserLength);
                if (hitSomthing)
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

    private bool CheckLazerCollision(float currentLength)
    {
        RaycastHit[] hits = Physics.RaycastAll(startPoint, direction, currentLength);
        bool hited = false;

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
            if(damagable != null)
            {
                damagable.OnDamage(damage);
                hited = true;
            }
        }

        return hited;
    }

}
