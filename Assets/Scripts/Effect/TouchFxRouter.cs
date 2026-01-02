using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TouchFxRouter : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] private FxId tapFxId = FxId.Ef_Tap;

    [Header("Filters")]
    [Tooltip("조이스틱 시작 영역(여기서 터치 시작되면 FX 스킵)")]
    [SerializeField] private RectTransform joystickTouchRect;
    [Tooltip("UI 위 터치면 스킵(원하면 false)")]
    [SerializeField] private bool ignoreWhenPointerOverUI = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private bool consumedThisTouch = false;

    private void Update()
    {
        var tm = TouchManager.Instance;
        //if (tm == null) return;
        if (tm == null) { if (debugLog) Debug.Log("[TouchFxRouter] TouchManager null"); return; }


        if (!tm.IsTouching || tm.TouchPhase == InputActionPhase.Canceled)
        {
            consumedThisTouch = false;
            return;
        }
        if (consumedThisTouch) return;

        consumedThisTouch = true;

        Vector2 pos = tm.TouchPos;

        if (tm.IsDragging)
        {
            if (debugLog) Debug.Log($"[TouchFxRouter] skip: dragging pos={pos}");
            return;
        }

        if (joystickTouchRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(joystickTouchRect, pos))
        {
            if (debugLog) Debug.Log($"[TouchFxRouter] skip: joystick area pos={pos}");
            return;
        }

        if (ignoreWhenPointerOverUI && EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            if (debugLog) Debug.Log($"[TouchFxRouter] skip: pointer over UI pos={pos}");
            return;
        }

        if (FxManager.Instance != null)
        {
            if (debugLog) Debug.Log($"[TouchFxRouter] PLAY FX id={tapFxId} pos={pos}");
            FxManager.Instance.PlayUI(tapFxId, pos);
        }
        else
        {
            if (debugLog) Debug.Log("[TouchFxRouter] FxManager null");
        }
    }
}
