using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerTargetingSystem : MonoBehaviour
{//updateTarget->GetEnemiesInRange(consider targetPriority) -> Return currentTarget -> driven atk sys

    [SerializeField] private Transform towerFiringPoint; //assign tower object
    public Transform FirePoint => towerFiringPoint;
    [SerializeField] private TargetRangeSO rangeData;
    [SerializeField] private BaseTargetPriority targetStrategy;

    private TowerDataSO assignedTowerData;
    private readonly string enemyTag = "Enemy";

    private ITargetable currentTarget;
    public ITargetable CurrentTarget => currentTarget;
    private ITargetable previousTarget; //only debug

    [SerializeField] private float scanInterval = 0.2f; // Multiple Interval Scan System
    private float scanTimer = 0f;

    private bool isAttacking { get; set; } = false;
    public void SetAttacking(bool value) => isAttacking = value;

    private void Awake()
    {
        if (towerFiringPoint == null) towerFiringPoint = transform;
    }

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
        if (rangeData == null||targetStrategy==null) return;

        float radius = rangeData.GetRange();
        Vector3 firingPoint = towerFiringPoint.position;

        Collider[] detects = Physics.OverlapSphere(firingPoint, radius, 
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        Debug.Log($"[TowerTargetingSystem] Collider count: {detects.Length}");

        var validTargets = new List<ITargetable>();
        foreach (var dt in detects)
        {
            if (dt.name == "Sphere") continue;
            
            Debug.Log($"[Scan] Detected: {dt.name} | Tag: {dt.tag} | IsTrigger: {dt.isTrigger} | Active: {dt.gameObject.activeInHierarchy}");

            if (!dt.CompareTag("Enemy")) continue;

            var targetComponent = dt.GetComponent<ITargetable>();

            if (targetComponent == null || !targetComponent.isAlive) continue;

            //Enemy Data Null Check
            var enemy = targetComponent as Enemy;
            if(enemy!=null&&enemy.Data==null)
            {
                Debug.LogWarning($"[TowerTargetingSystem] Enemy {dt.name} has null Data. Skipping...");
                continue;
            }
            
            validTargets.Add(targetComponent);
            
            Debug.Log($"[Scan] Valid Target: {dt.name} | HP:{targetComponent.maxHp} | ATK:{targetComponent.atk} | DEF:{targetComponent.def} | Pos:{targetComponent.position}");
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

        targetStrategy = data.targetPriority!=null
            ?ScriptableObject.Instantiate(data.targetPriority)
            :null;

        if(targetStrategy is ClosestDistancePrioritySO closest)
        {
            closest.Initialize(towerFiringPoint);
        }
    }
}