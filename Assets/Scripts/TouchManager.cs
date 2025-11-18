using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private static TouchManager instance;
    public static TouchManager Instance => instance;

    private Vector2 touchPos;
    public Vector2 TouchPos => touchPos;

    private bool isTouching;
    public bool IsTouching => isTouching;

    private InputActionPhase touchPhase;
    public InputActionPhase TouchPhase => touchPhase;

    public void OnUITouchPos(InputAction.CallbackContext context)
    {
        touchPos = context.ReadValue<Vector2>();
    }   

    public void OnUITouchCheck(InputAction.CallbackContext context)
    {
        isTouching = context.ReadValueAsButton();
    }

    public void OnUITouchPhase(InputAction.CallbackContext context)
    {
        touchPhase = context.phase;
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
}
