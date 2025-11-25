using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ProjectileVisibilityTracker : MonoBehaviour
{
    private Projectile projectile;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        projectile = GetComponentInParent<Projectile>();
    }

    private void OnBecameInvisible()
    {
        if (!Application.isPlaying) return;
        if (projectile == null) return;

        projectile.ForceFinish();
    }
}