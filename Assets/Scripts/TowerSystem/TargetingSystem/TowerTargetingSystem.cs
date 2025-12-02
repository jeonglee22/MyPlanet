using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class TowerTargetingSystem : MonoBehaviour
{
    [SerializeField] private TowerInstallControl towerInstallControl;
    [SerializeField] private Transform towerFiringPoint; //assign tower object
    public Transform FirePoint => towerFiringPoint;
    
    [SerializeField] private TargetRangeSO rangeData;
    [SerializeField] private BaseTargetPriority targetStrategy;

    private TowerDataSO assignedTowerData;

    //Single Target
    private ITargetable currentTarget;
    public ITargetable CurrentTarget => currentTarget;

    //Multi Target
    private readonly List<ITargetable> currentTargets = new List<ITargetable>();
    public IReadOnlyList<ITargetable> CurrentTargets => currentTargets;


    [SerializeField] private float scanInterval = 0.2f; // Multiple Interval Scan System
    private float scanTimer = 0f;

    private bool isAttacking { get; set; } = false;
    public void SetAttacking(bool value) => isAttacking = value;

    private int slotIndex = -1;
    public void SetSlotIndex(int index) => slotIndex = index;

    public TowerDataSO GetTowerData() => assignedTowerData;

    //Target Number
    private int baseMaxTargetCount = 1; //only Var
    private int extraTargetCount = 0; //Add Buffed Data
    public int MaxTargetCount => Mathf.Max(1, baseMaxTargetCount + extraTargetCount);

    private void Awake()
    {
        if (towerFiringPoint == null) towerFiringPoint = transform;
    }

    private void Start()
    {
        scanTimer = scanInterval;
    }

    private void Update()
    {
        scanTimer += Time.deltaTime;
        if(scanTimer>= scanInterval)
        {
            scanTimer = 0f;
            if (!isAttacking) ScanForTargets();
        }
    }

    private void ScanForTargets()
    {
        if (rangeData == null || targetStrategy == null)
        {
            currentTarget = null;
            currentTargets.Clear();
            return;
        }

        float radius = rangeData.GetRange();
        float radiusSqr = radius * radius;
        Vector3 firingPoint = towerFiringPoint.position;

        var visibleTargets = VisibleTargetManager.Instance != null
            ? VisibleTargetManager.Instance.VisibleTargets
            : null;

        currentTargets.Clear();
        currentTarget = null;

        if (visibleTargets == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        List<ITargetable> validTargets = new List<ITargetable>();

        foreach (var target in visibleTargets)
        {
            if (target == null) continue;
            if (!target.isAlive) continue;

            //Enemy Data Null Check
            var enemy = target as Enemy;
            if (enemy != null && enemy.Data == null) continue;

            //View Check
            Vector3 vp = cam.WorldToViewportPoint(target.position);
            bool inViewport = (vp.z > 0f &&
                               vp.x >= 0f && vp.x <= 1f &&
                               vp.y >= 0f && vp.y <= 1f);
            if (!inViewport) continue;

            Vector3 targetPos = target.position;
            float distSqr = (targetPos - firingPoint).sqrMagnitude;
            // if (distSqr > radiusSqr) continue;

            validTargets.Add(target);
        }

        if (validTargets.Count == 0)
        {
            currentTarget = null;
            return;
        }

        int maxTargets = MaxTargetCount;
        for(int i=0; i<maxTargets&& validTargets.Count>0; i++)
        {
            ITargetable best = targetStrategy.SelectTarget(validTargets);
            if (best == null) break;
            currentTargets.Add(best);
            validTargets.Remove(best);
        }
        currentTarget = currentTargets.Count > 0 ? currentTargets[0] : null;
    }
    public ITargetable GetCurrentTarget() => currentTarget;

    public void SetTowerData(TowerDataSO data)
    {
        assignedTowerData = data;
        rangeData = data.rangeData;

        targetStrategy = data.targetPriority != null
            ? ScriptableObject.Instantiate(data.targetPriority) : null;

        if (targetStrategy is ClosestDistancePrioritySO closest)
        {
            closest.Initialize(towerFiringPoint);
        }
    }
    public void SetExtraTargetCount(int extra)
    {
        extraTargetCount = extra;
    }

    public void AddExtraTargetCount(int extra)
    {
        extraTargetCount += extra;
    }

    public void ClearExtraTargetCount()
    {
        extraTargetCount = 0;
    }

    public void RemoveExtraTargetCount(int extra)
    {
        extraTargetCount = Mathf.Max(0, extraTargetCount - extra);
    }

    public void SetMaxTargetCount(int maxCount)
    {
        baseMaxTargetCount = Mathf.Max(1, maxCount);
        extraTargetCount = 0; 
    }
}