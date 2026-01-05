using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FxManager : MonoBehaviour
{
    public static FxManager Instance { get; private set; }

    [Header("Catalog (여기에만 등록)")]
    [SerializeField] private FxCatalog catalog;

    [Header("Parents")]
    [SerializeField] private Transform worldRoot;         // 없으면 자동 생성
    [SerializeField] private RectTransform uiRoot;        // 없으면 자동 생성
    [SerializeField] private Canvas uiCanvas;             // 비워도 됨(자동 탐색)

    private readonly ObjectPoolManager<FxId, PooledFx> pool = new();
    private readonly Dictionary<FxId, FxCatalog.Entry> map = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (worldRoot == null)
        {
            var go = new GameObject("FX_World_Root");
            worldRoot = go.transform;
            DontDestroyOnLoad(go);
        }

        if (uiRoot == null)
        {
            var go = new GameObject("FX_UI_Root", typeof(RectTransform));
            uiRoot = go.GetComponent<RectTransform>();
            DontDestroyOnLoad(go);
        }

        BuildMap();

        // 씬이 로드될 때마다 Canvas 다시 잡아서 uiRoot를 붙임
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindUIRoot();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindUIRoot();
    }

    private void RebindUIRoot()
    {
        // 1) 인스펙터에 지정된 uiCanvas가 “현재 씬에서도 유효”하면 우선 사용
        if (uiCanvas == null || !uiCanvas.gameObject.activeInHierarchy || !uiCanvas.enabled)
        {
            uiCanvas = FindBestCanvasInLoadedScenes();
        }

        if (uiCanvas != null)
        {
            uiRoot.SetParent(uiCanvas.transform, false);
            uiRoot.anchorMin = Vector2.zero;
            uiRoot.anchorMax = Vector2.one;
            uiRoot.offsetMin = Vector2.zero;
            uiRoot.offsetMax = Vector2.zero;

            Debug.Log($"[FxManager] UI Root bound to Canvas: {uiCanvas.name} (order={uiCanvas.sortingOrder}, mode={uiCanvas.renderMode})");
        }
        else
        {
            // Canvas를 못 찾으면 일단 루트는 유지 (PlayUI 때 또 찾도록 할 수도 있음)
            uiRoot.SetParent(null, false);
            Debug.LogWarning("[FxManager] No Canvas found. FX_UI_Root is unparented.");
        }
    }

    private Canvas FindBestCanvasInLoadedScenes()
    {
        // includeInactive=true : 씬에 있는 Canvas 다 찾기
        var canvases = FindObjectsOfType<Canvas>(true);
        Canvas best = null;
        int bestScore = int.MinValue;

        foreach (var c in canvases)
        {
            if (c == null) continue;
            if (!c.enabled || !c.gameObject.activeInHierarchy) continue;

            // 점수 기준: sortingOrder 높은 Canvas 우선
            int score = c.sortingOrder;

            // 보통 UI는 ScreenSpace가 많으니 살짝 가중치
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) score += 100000;
            else if (c.renderMode == RenderMode.ScreenSpaceCamera) score += 50000;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        return best;
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

    public PooledFx Play(FxId id, Vector3 worldPos, Quaternion rot = default)
    {
        EnsurePool(id);

        var fx = pool.Get(id);
        if (fx == null) return null;

        fx.Owner = this;
        fx.PoolKey = id;
        fx.transform.SetParent(worldRoot, false);
        fx.transform.position = worldPos;
        fx.transform.rotation = (rot == default) ? Quaternion.identity : rot;

        return fx;
    }

    public PooledFx PlayUI(FxId id, Vector2 screenPos)
    {
        // 혹시 씬 전환 직후 Canvas 못 잡았으면 여기서 한번 더
        if (uiCanvas == null || !uiCanvas.gameObject.activeInHierarchy || !uiCanvas.enabled)
            RebindUIRoot();

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

    public void Return(PooledFx fx)
    {
        if (fx == null) return;
        pool.Return(fx.PoolKey, fx);
    }

    public void ClearAllActiveFx()
    {
        ClearFxInParent(worldRoot);

        ClearFxInParent(uiRoot);
    }

    private void ClearFxInParent(Transform parent)
    {
        if(parent == null)
        {
            return;
        }

        for(int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            var pooledFx = child.GetComponent<PooledFx>();
            if(pooledFx != null && child.gameObject.activeSelf)
            {
                Return(pooledFx);
            }
        }
    }
}
