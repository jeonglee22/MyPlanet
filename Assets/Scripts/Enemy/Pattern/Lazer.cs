using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    [SerializeField] private float duration = 2f;
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private float laserWidth = 0.2f;
    [SerializeField] private float tickInterval = 0.1f;

    private BoxCollider boxCollider;
    private LineRenderer lineRenderer;
    
    private float damage;
    private Vector3 direction;
    private Vector3 startPoint;
    private Vector3 endPoint;

    private CancellationTokenSource lazerCts;

    private IDamagable damageTarget;

    public Action OnLazerEnd;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        Setup();
    }

    private void OnDisable()
    {
        Cancel();

        damageTarget = null;
    }

    private void OnDestroy()
    {
        Cancel();
    }

    private void Update()
    {
        
    }

    private void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.useWorldSpace = true;
    }

    public void Initialize(Vector3 startPosition, Vector3 direction, float damage, Action onEnd = null)
    {
        this.startPoint = startPosition;
        this.direction = direction.normalized;
        this.damage = damage;
        OnLazerEnd = onEnd;

        laserLength = CalculateDistance();

        endPoint = startPoint + this.direction * laserLength;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        SetupCollider();

        Cancel();

        LazerLifeCycleAsync(lazerCts.Token).Forget();
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
            float elapsedTime = 0f;
            float tickTimer = 0f;

            while (elapsedTime < duration)
            {
                token.ThrowIfCancellationRequested();

                elapsedTime += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if(tickTimer >= tickInterval && damageTarget != null)
                {
                    damageTarget.OnDamage(damage);
                    tickTimer = 0f;
                }

                await UniTask.Yield(cancellationToken: token);
            }
        }
        catch(System.OperationCanceledException)
        {
            
        }
        finally
        {
            OnLazerEnd?.Invoke();
            OnLazerEnd = null;

            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(TagName.Enemy) || other.CompareTag(TagName.Boss) || other.CompareTag(TagName.Projectile) || other.CompareTag(TagName.CenterStone) || other.CompareTag(TagName.PatternLine))
        {
            return;
        }

        var damagable = other.GetComponent<IDamagable>();
        if(damagable != null)
        {
            damageTarget = damagable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag(TagName.Enemy))
        {
            return;
        }

        var damagable = other.GetComponent<IDamagable>();
        if(damagable != null && damagable == damageTarget)
        {
            damageTarget = null;
        }
    }

    private float CalculateDistance()
    {
        Rect screenBounds = SpawnManager.Instance.ScreenBounds;

        float screenBottomY = screenBounds.yMin;
        float distanceBottom = startPoint.y - screenBottomY;

        return Mathf.Max(0.1f, distanceBottom);
    }

    private void SetupCollider()
    {
        Vector3 midPoint = (startPoint + endPoint) / 2f;

        transform.position = midPoint;

        if(direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }

        boxCollider.size = new Vector3(laserWidth, laserLength, 0.1f);
        boxCollider.center = Vector3.zero;
    }

    public void SetDuration(float duration) => this.duration = duration;
    public void SetTickInterval(float interval) => this.tickInterval = interval;
    public void SetLazerWidth(float width) => this.laserWidth = width;
}
