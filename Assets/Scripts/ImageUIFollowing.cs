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
        var touchPos = TouchManager.Instance.TouchPos;
        transform.position = touchPos;
    }
}
