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

    void Start()
    {
        isAppear = false;
        drag = joystick.GetComponentInChildren<OnScreenStick>();
    }

    void Update()
    {
        var touchScreen = Touchscreen.current;
        if (touchScreen == null) return;

        var primary = touchScreen.primaryTouch;

        if (primary.press.isPressed == false)
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

        var touchPos = primary.position.ReadValue();
        if (!RectTransformUtility.RectangleContainsScreenPoint(touchRect, touchPos) || EventSystem.current.IsPointerOverGameObject())
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
