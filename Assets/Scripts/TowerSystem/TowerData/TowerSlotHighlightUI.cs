using UnityEngine;
using UnityEngine.UI;

public enum TowerHighlightType
{
    None,               
    FromAttackSource,   // buff tower to attack tower
    BuffTarget,         // buff target
    RandomOnlyTarget    // ability target
}
public class TowerSlotHighlightUI : MonoBehaviour
{
    [SerializeField] private Image borderImage;

    [SerializeField] private Color fromAttackSourceColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] private Color buffTargetColor = new Color(1f, 0.85f, 0.3f); 
    [SerializeField] private Color randomOnlyColor = new Color(1f, 0.5f, 0.8f);

    [Header("Highlight Thickness")]
    [SerializeField] private float highlightScale = 1.08f;

    private Color defaultColor;
    private RectTransform borderRect;
    private Vector3 defaultScale;
    private bool initialized = false;

    private void Awake()
    {
        if (borderImage == null) borderImage = GetComponentInChildren<Image>();
        if (borderImage != null)
        {
            defaultColor = borderImage.color;
            borderRect = borderImage.rectTransform;
            defaultScale = borderRect.localScale;
            initialized = true;
        }
    }

    public void SetHighlight(TowerHighlightType type)
    {
        if (!initialized || borderImage == null || borderRect == null) return;

        switch (type)
        {
            case TowerHighlightType.None:
                borderImage.color = defaultColor;
                borderRect.localScale = defaultScale;
                break;

            case TowerHighlightType.FromAttackSource:
                borderImage.color = fromAttackSourceColor;
                borderRect.localScale = defaultScale * highlightScale;
                break;

            case TowerHighlightType.BuffTarget:
                borderImage.color = buffTargetColor;
                borderRect.localScale = defaultScale * highlightScale;
                break;

            case TowerHighlightType.RandomOnlyTarget:
                borderImage.color = randomOnlyColor;
                borderRect.localScale = defaultScale * highlightScale;
                break;
        }
    }
    public void RefreshDefaultColorFromImage()
    {
        if (borderImage == null || borderRect == null) return;

        defaultColor = borderImage.color;
        defaultScale = borderRect.localScale;
        initialized = true;
    }

}
