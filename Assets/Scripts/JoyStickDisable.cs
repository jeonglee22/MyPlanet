using UnityEngine;
using UnityEngine.InputSystem;

public class JoyStickDisable : MonoBehaviour
{
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        var touchScreen = Touchscreen.current;
        if (touchScreen == null)
        {
            gameObject.SetActive(false);
        }
    }
}
