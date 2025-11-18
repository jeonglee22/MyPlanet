using UnityEngine;
using UnityEngine.InputSystem;

public class PopUpUI : MonoBehaviour
{
    private RectTransform rectTransform;
    protected Vector2 touchPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnUITouchPos(InputAction.CallbackContext context)
    {
        touchPos = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos))
        {
            gameObject.SetActive(false);
        }
    }
}
