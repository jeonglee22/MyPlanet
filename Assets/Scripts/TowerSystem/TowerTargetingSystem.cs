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

    //debug
    private ITargetable previousTarget;

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
        Collider[] detects = Physics.OverlapSphere(towerFiringPoint.position, radius, 
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        Debug.Log($"collider count:{detects.Length}");
        var validTargets = new List<ITargetable>();
        foreach (var dt in detects)
        {
            Debug.Log($"[Scan] Detected: {dt.name} | Tag: {dt.tag} | IsTrigger: {dt.isTrigger} | Active: {dt.gameObject.activeInHierarchy}");
            if (!dt.CompareTag(enemyTag))
            {
                Debug.Log($"[Scan] Skipped {dt.name}, wrong tag");
                continue;
            }

            var targetComponent = dt.GetComponent<ITargetable>();
            if (targetComponent == null) Debug.Log($"No Target{dt.name}");
            if (!targetComponent.isAlive)
            {
                Debug.Log($"[Scan] {dt.name} is dead");
                continue;
            }

            validTargets.Add(targetComponent);
            Debug.Log($"[Scan] Valid Target: {dt.name} | HP:{targetComponent.maxHp} | ATK:{targetComponent.atk} | DEF:{targetComponent.def} | Pos:{targetComponent.position}");
            //if (targetComponent != null && targetComponent.isAlive)
        }

        currentTarget = targetStrategy != null 
            ? targetStrategy.SelectTarget(validTargets) : null;

        if (currentTarget !=previousTarget)
        {
            previousTarget = currentTarget;
            if (currentTarget != null) Debug.Log($"[New Best Target] ATK:{currentTarget.atk} DEF:{currentTarget.def} HP:{currentTarget.maxHp}");
            else Debug.Log("No Valid Target");
        }
    }
    public ITargetable GetCurrentTarget() => currentTarget;

    private void OnDrawGizmosSelected() //debug method
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
