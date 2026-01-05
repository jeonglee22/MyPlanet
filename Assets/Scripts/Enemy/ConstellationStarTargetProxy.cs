using UnityEngine;

/// <summary>
/// 별(자식)을 타게팅 대상으로 만들기 위한 ITargetable 프록시.
/// 데미지 처리는 안 함(피격/데미지는 기존 시스템이 부모로 전달한다고 가정).
/// </summary>
[DisallowMultipleComponent]
public class ConstellationStarTargetable : MonoBehaviour, ITargetable
{
    [Header("Owner (auto if null)")]
    [SerializeField] private Enemy ownerEnemy; 

    [Header("Register Mode")]
    [Tooltip("true: 화면에 보일 때만 VisibleTargetManager에 등록(OnBecameVisible 사용)\nfalse: Enable 시 항상 등록")]
    [SerializeField] private bool registerByVisibility = true;

    private Renderer rend;
    private bool isRegistered = false;

    private void Awake()
    {
        if (ownerEnemy == null) ownerEnemy = GetComponentInParent<Enemy>();
        rend = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (!registerByVisibility || rend == null)
            Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    private void OnBecameVisible()
    {
        if (!Application.isPlaying) return;
        if (!registerByVisibility) return;
        Register();
    }

    private void OnBecameInvisible()
    {
        if (!Application.isPlaying) return;
        if (!registerByVisibility) return;
        Unregister();
    }

    private void Register()
    {
        if (!Application.isPlaying) return;
        if (isRegistered) return;

        VisibleTargetManager.Instance?.Register(this);
        isRegistered = true;
    }

    private void Unregister()
    {
        if (!Application.isPlaying) return;
        if (!isRegistered) return;

        VisibleTargetManager.Instance?.Unregister(this);
        isRegistered = false;
    }
    public Vector3 position => transform.position;

    public bool isAlive
    {
        get
        {
            if (!gameObject.activeInHierarchy) return false;
            var ownerTarget = ownerEnemy as ITargetable;
            if (ownerTarget != null) return ownerTarget.isAlive;
            return true;
        }
    }

    public float maxHp => (ownerEnemy as ITargetable)?.maxHp ?? 0f;
    public float atk => (ownerEnemy as ITargetable)?.atk ?? 0f;
    public float def => (ownerEnemy as ITargetable)?.def ?? 0f;
}