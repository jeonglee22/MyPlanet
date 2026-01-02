using System.Collections.Generic;
using UnityEngine;

public class FxManager : MonoBehaviour
{
    public static FxManager Instance { get; private set; }

    [Header("Catalog (여기에만 등록)")]
    [SerializeField] private FxCatalog catalog;

    [Header("Parents")]
    [SerializeField] private Transform worldRoot;         // 없으면 자동 생성
    [SerializeField] private RectTransform uiRoot;        // 없으면 자동 생성
    [SerializeField] private Canvas uiCanvas;             // ScreenSpaceOverlay면 null 가능

    private readonly ObjectPoolManager<FxId, PooledFx> pool = new();
    private readonly Dictionary<FxId, FxCatalog.Entry> map = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (worldRoot == null)
        {
            var go = new GameObject("FX_World_Root");
            worldRoot = go.transform;
        }

        if (uiRoot == null)
        {
            var go = new GameObject("FX_UI_Root", typeof(RectTransform));
            uiRoot = go.GetComponent<RectTransform>();
            if (uiCanvas != null) uiRoot.SetParent(uiCanvas.transform, false);
        }

        BuildMap();
    }

    private void BuildMap()
    {
        map.Clear();
        if (catalog == null) return;

        foreach (var e in catalog.entries)
        {
            if (e == null || e.prefab == null) continue;
            map[e.id] = e;
        }
    }

    private void EnsurePool(FxId id)
    {
        if (pool.HasPool(id)) return;

        if (!map.TryGetValue(id, out var e) || e.prefab == null)
        {
            Debug.LogError($"[FxManager] FxId not registered: {id}. FxCatalog에 등록해줘.");
            return;
        }

        // prefab에 PooledFx가 꼭 있어야 함
        if (e.prefab.GetComponent<PooledFx>() == null)
        {
            Debug.LogError($"[FxManager] Prefab '{e.prefab.name}' has no PooledFx component. 루트에 PooledFx 붙여줘.");
            return;
        }

        Transform parent = e.isUI ? (Transform)uiRoot : worldRoot;

        pool.CreatePool(
            id,
            e.prefab,
            e.defaultPoolCapacity,
            e.maxPoolSize,
            e.collectionCheck,
            parent
        );
    }

    /// <summary>월드 FX 재생</summary>
    public PooledFx Play(FxId id, Vector3 worldPos, Quaternion rot = default)
    {
        EnsurePool(id);

        var fx = pool.Get(id);
        if (fx == null) return null;

        fx.Owner = this;
        fx.PoolKey = id;

        if (map.TryGetValue(id, out var e) && e.isUI)
        {
            Debug.LogWarning($"[FxManager] FxId '{id}' is UI FX. Use PlayUI() instead.");
            fx.transform.SetParent(uiRoot, false);
            if (fx.transform is RectTransform rt)
                rt.anchoredPosition = new Vector2(worldPos.x, worldPos.y);
        }
        else
        {
            fx.transform.SetParent(worldRoot, false);
            fx.transform.position = worldPos;
            fx.transform.rotation = (rot == default) ? Quaternion.identity : rot;
        }

        return fx;
    }


    /// <summary>UI FX 재생 (screenPos: Input.mousePosition 등)</summary>
    public PooledFx PlayUI(FxId id, Vector2 screenPos)
    {
        EnsurePool(id);

        var fx = pool.Get(id);
        if (fx == null) return null;

        fx.Owner = this;
        fx.PoolKey = id;

        fx.transform.SetParent(uiRoot, false);

        if (fx.transform is RectTransform rt)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiRoot,
                screenPos,
                uiCanvas != null ? uiCanvas.worldCamera : null,
                out var local
            );
            rt.anchoredPosition = local;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
        }
        else
        {
            fx.transform.position = screenPos;
            fx.transform.rotation = Quaternion.identity;
        }

        return fx;
    }

    /// <summary>PooledFx가 호출하는 Return</summary>
    public void Return(PooledFx fx)
    {
        if (fx == null) return;

        // pool.Return 내부에서 Dispose() + 비활성화 + parent 복귀까지 처리됨
        pool.Return(fx.PoolKey, fx);
    }
}
