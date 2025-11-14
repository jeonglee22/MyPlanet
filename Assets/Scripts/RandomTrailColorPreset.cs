using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class RandomTrailColorPreset : MonoBehaviour
{
    private TrailRenderer trail;

    [SerializeField]
    private Color[] colors =
    {
        Color.cyan,
        Color.green,
        Color.yellow,
        Color.magenta
    };

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        if (colors == null || colors.Length == 0) return;

        Color c = colors[Random.Range(0, colors.Length)];
        trail.startColor = c;
        trail.endColor = c * 0.5f;
    }
}