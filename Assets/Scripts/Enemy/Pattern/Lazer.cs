using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    protected float chargeTime = 3f;
    protected float duration = 2f;
    protected float laserLength = 10f;
    protected float laserWidth = 0.5f;
    protected float fieldWidth = 0.2f;
    protected float tickInterval = 0.1f;

    protected LineRenderer lineRenderer;
    protected SpriteRenderer fieldRenderer;
    
    private float damage;
    protected Vector3 direction;
    protected Vector3 startPoint;
    protected Vector3 endPoint;

    private CancellationTokenSource lazerCts;

    public Action OnLazerEnd;

    private IDamagable damageTarget;

    private AudioSource laserAudioSource;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        fieldRenderer = GetComponent<SpriteRenderer>();

        fieldRenderer.sortingOrder = -1;
    }

    private void OnEnable()
    {
        Setup();
    }

    private void OnDisable()
    {
        Cancel();
        StopLaserSound();
    }

    private void OnDestroy()
    {
        Cancel();
        StopLaserSound();
    }

    protected virtual void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.useWorldSpace = true;
    }

    private void StopLaserSound()
    {
        if(laserAudioSource != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopEnemyLaserLoop(laserAudioSource);
            laserAudioSource = null;
        }
    }

    public void Initialize(Vector3 startPosition, Vector3 direction, float damage, Action onEnd = null, float? customLength = null, CancellationToken token = default)
    {
        this.startPoint = startPosition;
        this.direction = direction.normalized;
        this.damage = damage;
        OnLazerEnd = onEnd;

        damageTarget = null;

        laserLength = customLength ?? CalculateDistance();

        endPoint = startPoint + this.direction * laserLength;

        lineRenderer.enabled = false;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        SetupField();

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
            await ChargePhaseAsync(token);

            await AttackPhaseAsync(token);
        }
        catch(System.OperationCanceledException)
        {
            
        }
        finally
        {
            OnLazerEnd?.Invoke();
            OnLazerEnd = null;

            if(this != null && gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private float CalculateDistance()
    {
        Rect screenBounds = SpawnManager.Instance.ScreenBounds;

        float screenBottomY = screenBounds.yMin;
        float distanceBottom = startPoint.y - screenBottomY;

        return Mathf.Max(0.1f, distanceBottom);
    }

    protected virtual void SetupField()
    {
        transform.position = startPoint;

        if(direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }

        fieldRenderer.transform.localScale = new Vector3(fieldWidth, laserLength, 1f);

        Vector3 midPoint = (startPoint + endPoint) / 2f;
        fieldRenderer.transform.localPosition = midPoint;
        fieldRenderer.transform.localRotation = transform.rotation;
    }

    public void SetDuration(float duration) => this.duration = duration;
    public void SetTickInterval(float interval) => this.tickInterval = interval;
    public void SetLazerWidth(float width) => this.laserWidth = width;

    private async UniTask ChargePhaseAsync(CancellationToken token)
    {
        float elapsedTime = 0f;

        fieldRenderer.enabled = true;

        Color startColor = new Color(1f, 0f, 0f, 0f);
        Color endColor = new Color(1f, 0f, 0f, 1f);

        while(elapsedTime < chargeTime)
        {
            token.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeTime;

            fieldRenderer.color = Color.Lerp(startColor, endColor, t);

            await UniTask.Yield(cancellationToken: token);
        }
    }

    private async UniTask AttackPhaseAsync(CancellationToken token)
    {
        fieldRenderer.enabled = false;
        lineRenderer.enabled = true;

        if(SoundManager.Instance != null && SoundManager.Instance.IsInitialized)
        {
            laserAudioSource = SoundManager.Instance.PlayEnemyLaserLoop(transform.position);
        }

        float elapsedTime = 0f;
        float tickTimer = 0f;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if(tickTimer >= tickInterval)
            {
                CheckLazerCollision();
                tickTimer = 0f;
            }

            await UniTask.Yield(cancellationToken: token);
        }

        lineRenderer.enabled = false;

        StopLaserSound();
    }

    private void CheckLazerCollision()
    {
        if(damageTarget != null)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(startPoint, direction, laserLength);

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

            var damagable = hit.collider.GetComponent<IDamagable>();
            if(damagable != null && damagable is Planet planet)
            {
                if(!planet.IsLazerHit)
                {
                    damagable.OnDamage(damage);
                    damageTarget = damagable;
                    planet.IsLazerHit = true;
                }
            }
        }
    }
}
