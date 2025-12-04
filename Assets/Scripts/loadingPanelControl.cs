using TMPro;
using UnityEngine;

public class loadingPanelControl : MonoBehaviour
{
    [SerializeField] private GameObject loadingImage;
    // [SerializeField] private TextMeshProUGUI loadingPercentText;
    private float rotatingTime;
    private float rotatingTimeInterval = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (SceneControlManager.Instance.IsLoading)
        {
            RotateLoadingImage();
        }
        else
        {
            loadingImage.transform.rotation = Quaternion.identity;
        }
    }

    private void RotateLoadingImage()
    {
        rotatingTime += Time.fixedDeltaTime;
        if (rotatingTime >= rotatingTimeInterval)
        {
            loadingImage.transform.Rotate(0f, 0f, 45f);
            rotatingTime = 0f;
        }
    }
}
