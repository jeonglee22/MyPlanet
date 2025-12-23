using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollSnapToCenter : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [Header("Snap")]
    [SerializeField] private float snapDuration = 0.18f; // 0.12~0.25 추천
    [SerializeField] private bool horizontal = true;
    [SerializeField] private float stopThreshold = 0.5f; // 너무 미세하게 떨면 0.5~2로

    [SerializeField] private LobbyUI lobbyUI;
    private readonly List<RectTransform> items = new();
    private CancellationTokenSource cts;

    private int choosedIndex = -1;
    public int ChoosedIndex => choosedIndex;

    // test
    private int startIndex = 1;

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        if (!viewport) viewport = scrollRect.viewport;
        if (!content) content = scrollRect.content;

        cts = new CancellationTokenSource();
    }

    private void RebuildItems()
    {
        items.Clear();
        for (int i = 0; i < content.childCount; i++)
            if (content.GetChild(i) is RectTransform rt) items.Add(rt);
    }

    private async void Start()
    {
        await UniTask.WaitUntil(() => lobbyUI.isInitialized);
        RebuildItems();

        await UniTask.Yield();
        SettingAtIndex(startIndex);
    }

    private void SettingAtIndex(int index)
    {
        choosedIndex = index;
        var nextRect = items[choosedIndex];

        Vector3 vpCenterWorld = viewport.TransformPoint(viewport.rect.center);

        Vector3 nearestCenterWorld = nextRect.TransformPoint(nextRect.rect.center);
        Vector3 deltaWorld = vpCenterWorld - nearestCenterWorld;
        Vector3 deltaLocal = viewport.InverseTransformVector(deltaWorld);

        Vector2 target = content.anchoredPosition + new Vector2(
            horizontal ? deltaLocal.x : 0f,
            horizontal ? 0f : deltaLocal.y
        );

        content.anchoredPosition = target;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CancelSnap();
        cts = new CancellationTokenSource();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (items.Count == 0) return;

        Vector3 vpCenterWorld = viewport.TransformPoint(viewport.rect.center);

        RectTransform nearest = null;
        float best = float.MaxValue;

        int index = -1;
        Debug.Log(items.Count);
        foreach (var it in items)
        {
            Vector3 itemCenterWorld = it.TransformPoint(it.rect.center);
            float d = horizontal
                ? Mathf.Abs(itemCenterWorld.x - vpCenterWorld.x)
                : Mathf.Abs(itemCenterWorld.y - vpCenterWorld.y);

            if (d < best)
            {
                best = d;
                nearest = it;
                index = items.IndexOf(it);
            }
        }

        if (index == -1) return;

        choosedIndex = index;

        if (nearest == null) return;

        Vector3 nearestCenterWorld = nearest.TransformPoint(nearest.rect.center);
        Vector3 deltaWorld = vpCenterWorld - nearestCenterWorld;
        Vector3 deltaLocal = viewport.InverseTransformVector(deltaWorld);

        Vector2 target = content.anchoredPosition + new Vector2(
            horizontal ? deltaLocal.x : 0f,
            horizontal ? 0f : deltaLocal.y
        );

        scrollRect.velocity = Vector2.zero;

        SnapTo(target, cts.Token).Forget();

        Debug.Log("Snapped to index: " + choosedIndex);
    }

    public async UniTask SnapLeftOne()
    {
        if (choosedIndex == 1)
            return;

        choosedIndex--;
        var nextRect = items[choosedIndex];

        Vector3 vpCenterWorld = viewport.TransformPoint(viewport.rect.center);

        Vector3 nearestCenterWorld = nextRect.TransformPoint(nextRect.rect.center);
        Vector3 deltaWorld = vpCenterWorld - nearestCenterWorld;
        Vector3 deltaLocal = viewport.InverseTransformVector(deltaWorld);

        Vector2 target = content.anchoredPosition + new Vector2(
            horizontal ? deltaLocal.x : 0f,
            horizontal ? 0f : deltaLocal.y
        );

        scrollRect.velocity = Vector2.zero;

        await SnapTo(target, cts.Token);
    }

    public async UniTask SnapRightOne()
    {
        if (choosedIndex == items.Count - 2)
            return;

        choosedIndex++;
        var nextRect = items[choosedIndex];

        Vector3 vpCenterWorld = viewport.TransformPoint(viewport.rect.center);

        Vector3 nearestCenterWorld = nextRect.TransformPoint(nextRect.rect.center);
        Vector3 deltaWorld = vpCenterWorld - nearestCenterWorld;
        Vector3 deltaLocal = viewport.InverseTransformVector(deltaWorld);

        Vector2 target = content.anchoredPosition + new Vector2(
            horizontal ? deltaLocal.x : 0f,
            horizontal ? 0f : deltaLocal.y
        );

        scrollRect.velocity = Vector2.zero;

        await SnapTo(target, cts.Token);
    }

    private async UniTask SnapTo(Vector2 target, CancellationToken token)
    {
        Vector2 start = content.anchoredPosition;

        if ((target - start).magnitude <= stopThreshold)
        {
            content.anchoredPosition = target;
            return;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, snapDuration);

        while (elapsed < duration)
        {
            token.ThrowIfCancellationRequested();

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            content.anchoredPosition = Vector2.LerpUnclamped(start, target, eased);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        content.anchoredPosition = target;
    }

    private void CancelSnap()
    {
        if (cts == null) return;
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    private void OnDestroy()
    {
        CancelSnap();
    }
}
