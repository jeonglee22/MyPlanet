using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SkillBasedLazer : MonoBehaviour
{
    protected float chargeTime = 1.5f;
    protected float laserLength = 10f;
    protected float laserWidth = 0.5f;
    protected float fieldWidth = 0.2f;
    protected float expandSpeed = 5f;

    protected LineRenderer lineRenderer;
    protected SpriteRenderer fieldRenderer;

    protected float damage;
    protected float tickInterval;
    protected float duration;
    protected int repeatCount;
    protected Vector3 direction;
    protected Vector3 startPoint;
    protected Vector3 endPoint;

    private Transform targetTransform;
    private int effectArrive;

    protected SkillData skillData;
    protected CancellationTokenSource lazerCts;

    public event Action OnLazerEnd;

    protected ParticleSystem laserParticle;

    protected Transform ownerTransform;
    protected float particleOffsetY;

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

    protected abstract void Setup();

    protected virtual void Initialize(Vector3 startPosition, Transform target, float damage, SkillData skillData, Transform owner = null, Action onEnd = null, CancellationToken token = default)
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

        CalculateLaserLength();

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

    protected virtual void CalculateLaserLength()
    {
        Rect screenBounds = SpawnManager.Instance.ScreenBounds;
        float screenBottomY = screenBounds.yMin;
        float distanceBottom = startPoint.y - screenBottomY;
        laserLength = Mathf.Max(0.1f, distanceBottom);

        endPoint = startPoint + direction * laserLength;
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

    protected virtual async UniTask ChargePhaseWithTrackAsync(CancellationToken token)
    {
        await UniTask.Yield(token);
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

    protected virtual async UniTask ExpandingAttackPhaseAsync(CancellationToken token)
    {
        fieldRenderer.enabled = false;
        lineRenderer.enabled = true;

        transform.position = startPoint;

        SetupParticle();

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

    protected virtual void UpdateParticleSpeed(float currentLength)
    {
        if(laserParticle != null)
        {
            var main = laserParticle.main;
            main.startSpeed = currentLength;
        }
    }

    protected virtual void SetupParticle()
    {
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
    }
}
