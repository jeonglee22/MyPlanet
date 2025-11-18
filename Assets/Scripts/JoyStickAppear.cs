using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class JoyStickAppear : MonoBehaviour
{
    public GameObject joystick;
    public RectTransform touchRect;

    private bool isAppear;
    private PointerEventData eventData;
    private OnScreenStick drag;

    private Vector2 touchPos;
    private bool isTouching;

    void Start()
    {
        isAppear = false;
        joystick.SetActive(false);
        drag = joystick.GetComponentInChildren<OnScreenStick>();
    }

    public void OnUITouchPos(InputAction.CallbackContext context)
    {
        touchPos = context.ReadValue<Vector2>();
    }

    public void OnUITouchCheck(InputAction.CallbackContext context)
    {
        isTouching = context.ReadValueAsButton();
    }

    void Update()
    {
        if(EventSystem.current.IsPointerOverGameObject())
            return;

        if (!isTouching)
        {
            if (isAppear)
            {
                eventData = new PointerEventData(EventSystem.current);
                drag.OnPointerUp(eventData);
                isAppear = false;
                joystick.SetActive(false);
            }
            return;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(touchRect, touchPos))
        {
            return;
        }

        if (isAppear)
        {
            eventData = new PointerEventData(EventSystem.current) { position = touchPos, };
            drag.OnDrag(eventData);
            return;
        }
        else
        {
            joystick.transform.position = touchPos;
            joystick.SetActive(true);

            eventData = new PointerEventData(EventSystem.current) { position = touchPos, };
            drag.OnPointerDown(eventData);
            isAppear = true;
        }
            
    }
}
