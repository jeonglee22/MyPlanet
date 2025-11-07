using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class JoyStickAppear : MonoBehaviour
{
    public GameObject joystick;
    public RectTransform touchRect;

    private bool isAppear;
    private PointerEventData eventData;

    void Start()
    {
        isAppear = false;
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

         var drag = joystick.GetComponentInChildren<OnScreenStick>();

        if (Input.touchCount != 0 && (Input.GetTouch(0).phase == TouchPhase.Ended ||
            Input.GetTouch(0).phase == TouchPhase.Canceled))
        {
            eventData = new PointerEventData(EventSystem.current);
            drag.OnPointerUp(eventData);
            isAppear = false;
            return;
        }

        var touchPos = Input.GetTouch(0).position;
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
