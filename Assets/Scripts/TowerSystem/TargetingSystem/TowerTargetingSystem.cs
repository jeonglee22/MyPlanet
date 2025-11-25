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

    private ITargetable currentTarget;
    public ITargetable CurrentTarget => currentTarget;
    private ITargetable previousTarget; //only debug

    [SerializeField] private float scanInterval = 0.2f; // Multiple Interval Scan System
    private float scanTimer = 0f;

    private bool isAttacking { get; set; } = false;
    public void SetAttacking(bool value) => isAttacking = value;

    private int slotIndex = -1;
    public void SetSlotIndex(int index) => slotIndex = index;

    public TowerDataSO GetTowerData() => assignedTowerData;

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
        if (rangeData == null || targetStrategy == null) return;

        float radius = rangeData.GetRange();
        float radiusSqr = radius * radius;
        Vector3 firingPoint = towerFiringPoint.position;

        var validTargets = new List<ITargetable>();
        int totalEnemiesInRange = 0;

        var visibleTargets = VisibleTargetManager.Instance != null ?
            VisibleTargetManager.Instance.VisibleTargets : null;

        if(visibleTargets==null)
        {
            currentTarget = null;
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            currentTarget = null;
            return;
        }

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
            if (distSqr > radiusSqr) continue;

            totalEnemiesInRange++;
            validTargets.Add(target);
        }

        currentTarget = (targetStrategy != null&&validTargets.Count>0) 
            ? targetStrategy.SelectTarget(validTargets) : null;

        if (currentTarget != null)
        {
            var mb = currentTarget as MonoBehaviour;
            string targetName = mb != null ? mb.name : "Unknown";

            float dist = Vector3.Distance(currentTarget.position, firingPoint);
            float radiuss = rangeData.GetRange();
        }
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
}