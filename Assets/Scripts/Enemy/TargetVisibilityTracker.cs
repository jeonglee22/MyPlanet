using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TargetableVisibilityTracker : MonoBehaviour
{
    private ITargetable target;
    private Renderer rend;
    private bool isRegistered = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        target = GetComponentInParent<ITargetable>();
    }

    private void OnBecameVisible()
    {
        if (!Application.isPlaying) return;
        if (target == null) return;
        if (isRegistered) return;

        VisibleTargetManager.Instance?.Register(target);
        isRegistered = true;
    }

    private void OnBecameInvisible()
    {
        if (!Application.isPlaying) return;
        if (target == null) return;
        if (!isRegistered) return;

        VisibleTargetManager.Instance?.Unregister(target);
        isRegistered = false;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (target == null) return;
        if (!isRegistered) return;

        VisibleTargetManager.Instance?.Unregister(target);
        isRegistered = false;
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        if (target == null) return;
        if (!isRegistered) return;

        VisibleTargetManager.Instance?.Unregister(target);
        isRegistered = false;
    }
}