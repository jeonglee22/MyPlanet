using UnityEngine;
using TMPro;
using System.Collections;
using Cysharp.Threading.Tasks;

public class NewBadgeAnimator : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private RectTransform badgeRoot;   // New! 오브젝트(부모)
    [SerializeField] private TextMeshProUGUI label;            // New! 텍스트 (선택)

    [Header("Pop (one-shot)")]
    [SerializeField] private float popDuration = 0.18f;
    [SerializeField] private float popOvershoot = 1.15f;

    [Header("Idle (loop)")]
    [SerializeField] private float pulseScale = 0.06f;  // 0.06 = 6% 정도
    [SerializeField] private float pulseSpeed = 3.5f;
    [SerializeField] private float wiggleDeg = 6f;
    [SerializeField] private float wiggleSpeed = 2.8f;

    [Header("Optional")]
    [SerializeField] private bool useUnscaledTime = true; // 일시정지 UI면 true 추천
    [SerializeField] private bool softBlink = false;
    [SerializeField] private float blinkAmount = 0.15f;   // 0.15면 85~100% 사이
    [SerializeField] private float blinkSpeed = 2.2f;

    private Vector3 baseScale;
    private Quaternion baseRot;
    private float t;

    private void Reset()
    {
        badgeRoot = GetComponent<RectTransform>();
        label = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (badgeRoot == null) return;

        baseScale = Vector3.one;
        baseRot = Quaternion.identity;

        badgeRoot.localScale = Vector3.zero;
        badgeRoot.localRotation = baseRot;
        t = 0f;

        // 팝 시작
        PopRoutine().Forget();
    }

    private async UniTask PopRoutine()
    {
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += dt;

            float x = Mathf.Clamp01(elapsed / popDuration);

            // EaseOutBack 비슷한 느낌(오버슈트)
            float s = EaseOutBack(x, popOvershoot);

            badgeRoot.localScale = baseScale * s;

            await UniTask.Yield();
        }

        badgeRoot.localScale = baseScale; // 정착
    }

    private void Update()
    {
        if (badgeRoot == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        t += dt;

        // 펄스 스케일
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseScale;

        // 살짝 흔들림
        float wiggle = Mathf.Sin(t * wiggleSpeed) * wiggleDeg;

        badgeRoot.localScale = baseScale * pulse;
        badgeRoot.localRotation = Quaternion.Euler(0f, 0f, wiggle);

        // 선택: 소프트 블링크
        if (softBlink && label != null)
        {
            var c = label.color;
            float a = 1f - (Mathf.Sin(t * blinkSpeed) * 0.5f + 0.5f) * blinkAmount;
            c.a = a;
            label.color = c;
        }
    }

    private float EaseOutBack(float x, float overshoot)
    {
        // overshoot: 1.1~1.3 권장
        float c1 = 1.70158f * overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    public void SetVisible(bool isNew)
    {
        gameObject.SetActive(isNew);
    }
}
