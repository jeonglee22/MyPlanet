using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerTargetingSystem : MonoBehaviour
{//updateTarget->GetEnemiesInRange(consider targetPriority) -> Return currentTarget -> driven atk sys

    [SerializeField] private Transform towerFiringPoint; //assign tower object
    [SerializeField] private TargetRangeSO rangeData;
    [SerializeField] private BaseTargetPriority targetStrategy;

    private TowerDataSO assignedTowerData;
    public TowerDataSO AssignedTowerData => assignedTowerData;

    private readonly string enemyTag = "Enemy";
    private ITargetable currentTarget;

    [SerializeField] private float scanInterval = 0.2f; // Multiple Interval Scan System
    private float scanTimer = 0f;
    private bool isAttacking { get; set; } = false;

    private void Update()
    {
        scanTimer += Time.deltaTime;
        if(scanTimer>= scanInterval)
        {
            scanTimer = 0f;
            if(!isAttacking) ScanForTargets();
        }
    }

    private void ScanForTargets()
    {
        float radius = rangeData.GetRange();
        Collider[] detects = Physics.OverlapSphere(towerFiringPoint.position, radius);
        var validTargets = new List<ITargetable>();
        foreach (var dt in detects)
        {
            if (!dt.CompareTag(enemyTag)) continue;

            var targetComponent = dt.GetComponent<ITargetable>();
            if (targetComponent != null && targetComponent.isAlive)
            {
                validTargets.Add(targetComponent);
            }
        }

        currentTarget = targetStrategy != null 
            ? targetStrategy.SelectTarget(validTargets) : null;
    }
    public ITargetable GetCurrentTarget() => currentTarget;

    private void OnDrawGizmosSelected() //debug
    {
        if(towerFiringPoint!=null&&rangeData!=null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(towerFiringPoint.position, rangeData.GetRange());
        }
    }

    public void SetTowerData(TowerDataSO data)
    {
        assignedTowerData = data;
        rangeData = data.rangeData;
        targetStrategy = data.targetPriority;

        Debug.Log($"{name}:");
    }
}
