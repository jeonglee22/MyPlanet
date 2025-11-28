using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ProjectileVisibilityTracker : MonoBehaviour
{
    private Projectile projectile;
    private Renderer rend;
    private Camera cam;
    private bool wasVisible = false;

    [Range(0f, 0.2f)]
    [SerializeField] private float borderOffset = 0.0f;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        projectile = GetComponent<Projectile>();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || projectile == null) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = rend.bounds.center;
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        if (vp.z <= 0f)
        {
            if (wasVisible)
                projectile.ForceFinish();
            return;
        }

        float minX = 0f - borderOffset;
        float maxX = 1f + borderOffset;
        float minY = 0f - borderOffset;
        float maxY = 1f + borderOffset;

        bool isVisible =
            vp.x >= minX && vp.x <= maxX &&
            vp.y >= minY && vp.y <= maxY;

        if (wasVisible && !isVisible)
        {
            projectile.ForceFinish();
        }

        if (isVisible) wasVisible = true;
    }
}