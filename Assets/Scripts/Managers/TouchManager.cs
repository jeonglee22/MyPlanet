using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private static TouchManager instance;
    public static TouchManager Instance => instance;

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
    }   

    public void OnUITouchPhase(InputAction.CallbackContext context)
    {
        touchPhase = context.phase;
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
