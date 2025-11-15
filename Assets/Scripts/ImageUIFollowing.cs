using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImageUIFollowing : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI testText;
    [SerializeField] private Image testImage;

    // Update is called once per frame
    void Update()
    {
        var touchScreen = Touchscreen.current;
        if (touchScreen == null) return;

        var primary = touchScreen.primaryTouch;

        if (primary.press.isPressed == false)
        {
            Destroy(gameObject);
            return;
        }

        var touchPos = primary.position.ReadValue();
        transform.position = touchPos;
    }
}
