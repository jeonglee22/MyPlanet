using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImageUIFollowing : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI testText;
    [SerializeField] private Image testImage;

    private Vector2 touchPos;

    public void OnTouchFollowUI(InputAction.CallbackContext context)
    {
        touchPos = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = touchPos;
    }
}
