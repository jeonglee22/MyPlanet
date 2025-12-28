
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpAndDestroyPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    private float displayDuration = 2f;
    private float movingUpSpeed = 40f;
    private float elapsedTime = 0f;
    private Image[] images;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        images = GetComponentsInChildren<Image>();
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;
        transform.Translate(Vector3.up * movingUpSpeed * Time.deltaTime);
        ImageFading();

        if (elapsedTime >= displayDuration)
        {
            Destroy(gameObject);
        }        
    }

    private void ImageFading()
    {
        foreach (var img in images)
        {
            var color = img.color;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / displayDuration);
            img.color = color;
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}   
