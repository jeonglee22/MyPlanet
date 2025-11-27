using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UICameraViewportFitter : MonoBehaviour
{
    public Camera targetCamera;
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Rect r = targetCamera.pixelRect;

        Vector2 anchorMin = new Vector2(
            r.xMin / Screen.width,
            r.yMin / Screen.height);

        Vector2 anchorMax = new Vector2(
            r.xMax / Screen.width,
            r.yMax / Screen.height);

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
