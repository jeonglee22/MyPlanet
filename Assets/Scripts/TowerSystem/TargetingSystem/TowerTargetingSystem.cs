using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class TowerTargetingSystem : MonoBehaviour
{//updateTarget->GetEnemiesInRange(consider targetPriority) -> Return currentTarget -> driven atk sys


    [SerializeField] private TowerInstallControl towerInstallControl;
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
            if(!isAttacking) ScanForTargets();
        }
    }

    private void ScanForTargets()
    {
        if (rangeData == null||targetStrategy==null)
        {
            Debug.LogWarning($"[TowerTargetingSystem] {gameObject.name}: RangeData or TargetStrategy is null.");
            return;
        }

        float radius = rangeData.GetRange();
        Vector3 firingPoint = towerFiringPoint.position;

        Collider[] detects = Physics.OverlapSphere(firingPoint, radius, 
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        Debug.Log($"[TowerTargetingSystem] Collider count: {detects.Length}");

        var validTargets = new List<ITargetable>();
        int totalEnemiesInRange = 0;

        foreach (var dt in detects)
        {
            if (dt.name == "Sphere") continue;

            if (!dt.CompareTag("Enemy")) continue;
            totalEnemiesInRange++;

            var targetComponent = dt.GetComponent<ITargetable>();

            if (targetComponent == null)
            {
                Debug.LogWarning($"[TowerTargetingSystem] {dt.name} has no ITargetable component. Skipping...");
                continue;
            }

            //Enemy Data Null Check
            var enemy = targetComponent as Enemy;
            if(enemy!=null&&enemy.Data==null)
            {
                Debug.LogWarning($"[TowerTargetingSystem] Enemy {dt.name} has null Data. Skipping...");
                continue;
            }

            if (!targetComponent.isAlive)
            {
                Debug.Log($"[TowerTargetingSystem] Enemy {dt.name} is not alive. Skipping...");
                continue;
            }

            validTargets.Add(targetComponent);
            
            Debug.Log($"[Scan] Valid Target: {dt.name} | HP:{targetComponent.maxHp} | ATK:{targetComponent.atk} | DEF:{targetComponent.def} | Pos:{targetComponent.position}");
        }

        currentTarget = targetStrategy != null 
            ? targetStrategy.SelectTarget(validTargets) : null;

        //Debug Log
        string slotIndexStr = slotIndex >= 0 ? slotIndex.ToString() : "Unknown";
        string priorityName = targetStrategy != null ? targetStrategy.name : "None";
        string rangeName = rangeData != null ? rangeData.name : "None";

        if (currentTarget != previousTarget)
        {
            previousTarget = currentTarget;
            string targetName = (currentTarget as MonoBehaviour)?.name ?? "Unknown";

            if (currentTarget != null)
                Debug.Log($"[BestTarget] Tower '{gameObject.name}' Slot {slotIndexStr} | Priority: {priorityName} | Range: {rangeName} (Enemies in Range: {totalEnemiesInRange}) => New Best Target: {targetName} | HP:{currentTarget.maxHp} ATK:{currentTarget.atk} DEF:{currentTarget.def}");
            else
                Debug.Log($"[BestTarget] Tower '{gameObject.name}' Slot {slotIndexStr} | Priority: {priorityName} | Range: {rangeName} (Enemies in Range: {totalEnemiesInRange}) => No valid target");
        }
    }
    public ITargetable GetCurrentTarget() => currentTarget;

    public void SetTowerData(TowerDataSO data)
    {
        assignedTowerData = data;
        rangeData = data.rangeData;

        targetStrategy = data.targetPriority!=null
            ?ScriptableObject.Instantiate(data.targetPriority):null;

        if(targetStrategy is ClosestDistancePrioritySO closest)
        {
            closest.Initialize(towerFiringPoint);
        }
    }
}