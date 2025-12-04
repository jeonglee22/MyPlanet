using UnityEngine;

public class TextPoint : MonoBehaviour
{
    [SerializeField] private TutorialPoint pointType;

    public TutorialPoint PointType => pointType;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public RectTransform GetRectTransform()
    {
        if(rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        return rectTransform;
    }
}
