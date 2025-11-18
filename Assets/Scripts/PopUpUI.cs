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

    // Update is called once per frame
    protected virtual void Update()
    {
        touchPos = TouchManager.Instance.TouchPos;
        if(!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos))
        {
            gameObject.SetActive(false);
        }
    }
}
