using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private static TouchManager instance;
    public static TouchManager Instance => instance;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;
    private bool lastTouching;
    private InputActionPhase lastPhase;

    private Vector2 touchPos;
    public Vector2 TouchPos => touchPos;

    private Vector2 startTouchPos;
    public Vector2 StartTouchPos => startTouchPos;
    private bool isStartTouchPosSet = false;

    private bool isTouching;
    public bool IsTouching => isTouching;

    private InputActionPhase touchPhase;
    public InputActionPhase TouchPhase => touchPhase;

    private float holdingTime = 1f;
    private float holdingTimer = 0f;
    private float draggingOffset = 20f;

    private bool isHolding;
    public bool IsHolding => isHolding;

    private bool isDragging;
    public bool IsDragging => isDragging;

    public void OnUITouchPos(InputAction.CallbackContext context)
    {
        touchPos = context.ReadValue<Vector2>();
        if (debugLog && (context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed))
        {
            Debug.Log($"[TouchManager][Pos] phase={context.phase} pos={touchPos} action={context.action?.name} control={context.control?.path}");
        }
    }   

    public void OnUITouchPhase(InputAction.CallbackContext context)
    {   //debug
        touchPhase = context.phase;
        bool asButton = context.ReadValueAsButton();
        float asFloat = 0f;
        try { asFloat = context.ReadValue<float>(); } catch { /* action 타입이 float이 아니면 예외 */ }

        if (touchPhase == InputActionPhase.Canceled)
        {
            isTouching = false;
        }
        else if (touchPhase == InputActionPhase.Started)
        {
            isTouching = true;
        }

        if (!context.ReadValueAsButton())
            isTouching = false;
        //debug
        if (!asButton)
            isTouching = false;

        if (debugLog)
        {
            Debug.Log(
                $"[TouchManager][Phase] phase={touchPhase} isTouching={isTouching} " +
                $"ReadValueAsButton={asButton} ReadValue<float>={asFloat:0.###} " +
                $"action={context.action?.name} control={context.control?.path}"
            );
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        if (debugLog && (lastTouching != isTouching || lastPhase != touchPhase))
        {
            Debug.Log($"[TouchManager][State] isTouching {lastTouching} -> {isTouching}, phase {lastPhase} -> {touchPhase}");
            lastTouching = isTouching;
            lastPhase = touchPhase;
        }

        if (!isTouching)
        {
            holdingTimer = 0f;
            isStartTouchPosSet = false;
            isHolding = false;
            isDragging = false;
            return;
        }

        if(!isStartTouchPosSet)
        {
            startTouchPos = touchPos;
            isStartTouchPosSet = true;
        }

        if(Vector2.Distance(startTouchPos, touchPos) > draggingOffset)
        {
            holdingTimer = 0f;
            isHolding = false;
            isDragging = true;
        }
        else
        {
            holdingTimer += Time.unscaledDeltaTime;
            if (holdingTimer > holdingTime)
            {
                isHolding = true;
            }
            isDragging = false;
        }
    }
}
