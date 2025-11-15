using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpUI : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        var touchScreen = Touchscreen.current;
        if (touchScreen == null) return;

        var primary = touchScreen.primaryTouch;
        if (!primary.press.isPressed) return;

        var touchPos = primary.position.ReadValue();

        if(!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos))
        {
            gameObject.SetActive(false);
        }
    }
}
