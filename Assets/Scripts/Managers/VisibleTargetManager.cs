using System.Collections.Generic;
using UnityEngine;

public class VisibleTargetManager : MonoBehaviour
{
    public static VisibleTargetManager Instance { get; private set; }

    private readonly HashSet<ITargetable> visibleTargets = new HashSet<ITargetable>();
    public IReadOnlyCollection<ITargetable> VisibleTargets => visibleTargets;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(ITargetable target)
    {
        if (target == null) return;
        visibleTargets.Add(target);
    }

    public void Unregister(ITargetable target)
    {
        if (target == null) return;
        visibleTargets.Remove(target);
    }
}