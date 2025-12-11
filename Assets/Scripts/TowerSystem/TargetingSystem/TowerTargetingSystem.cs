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
    public int BaseTargetCount => Mathf.Max(1, baseMaxTargetCount);

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
        if (targetStrategy == null)
        {
            currentTarget = null;
            currentTargets.Clear();
            return;
        }

        //float radius = rangeData != null ? rangeData.GetRange() : 999f;
        //float radiusSqr = radius * radius;
        Vector3 firingPoint = towerFiringPoint.position;

        var visibleTargets = VisibleTargetManager.Instance != null
            ? VisibleTargetManager.Instance.VisibleTargets
            : null;

        currentTargets.Clear();
        currentTarget = null;

        if (visibleTargets == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        RangeType preferredRangeType = rangeData != null ? rangeData.RangeType : RangeType.Short;

        List<ITargetable> validTargets = new List<ITargetable>();
        List<(ITargetable target, float distSqr)> allAreaTargets = new List<(ITargetable, float)>();

        foreach (var target in visibleTargets)
        {
            if (target == null) continue;
            if (!target.isAlive) continue;

            //Boss Enemy
            var enemy = target as Enemy;
            if(enemy!=null)
            {
                if (!enemy.IsTargetable) continue;
                if (enemy.Data == null) continue;
                if (enemy.Data.EnemyType == 4 && Variables.MiddleBossEnemy != null) continue;
            }

            //View Check
            Vector3 vp = cam.WorldToViewportPoint(target.position);
            bool inViewport = (vp.z > 0f &&
                               vp.x >= 0f && vp.x <= 1f &&
                               vp.y >= 0f && vp.y <= 1f);
            if (!inViewport) continue;

            Vector3 targetPos = target.position;
            float distSqr = (targetPos - firingPoint).sqrMagnitude;
            // if (distSqr > radiusSqr) continue;
            
            RangeType targetAreaType = GetRangeTypeFromViewportY(vp.y);

            allAreaTargets.Add((target, distSqr));

            if(targetAreaType==preferredRangeType)
            {
                validTargets.Add(target);
            }
        }

        if (validTargets.Count == 0&&allAreaTargets.Count==0)
        {
            currentTarget = null;
            return;
        }

        int maxTargets = MaxTargetCount;

        if (validTargets.Count > 0)
        {
            List<ITargetable> candidates = validTargets;

            for (int i = 0; i < maxTargets && candidates.Count > 0; i++)
            {
                ITargetable best = targetStrategy.SelectTarget(candidates);
                if (best == null) break;

                currentTargets.Add(best);
                candidates.Remove(best);
            }
        }
        else
        {
            allAreaTargets.Sort((a, b) => a.distSqr.CompareTo(b.distSqr));

            for (int i = 0; i < maxTargets && i < allAreaTargets.Count; i++)
            {
                currentTargets.Add(allAreaTargets[i].target);
            }
        }

/*        for (int i=0; i<maxTargets&& validTargets.Count>0; i++)
        {
            ITargetable best = targetStrategy.SelectTarget(validTargets);
            if (best == null) break;
            currentTargets.Add(best);
            validTargets.Remove(best);
        }*/
        currentTarget = currentTargets.Count > 0 ? currentTargets[0] : null;
        Debug.Log(
    $"[ScanForTargets] {gameObject.name} " +
    $"maxTargets={MaxTargetCount}, pickedTargets={currentTargets.Count}"
);
    }

    private RangeType GetRangeTypeFromViewportY(float y)
    {
        if (y < 1f / 3f) return RangeType.Short;
        else if (y < 2f / 3f) return RangeType.Mid;
        else return RangeType.Long;
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
    public void SetBaseTargetCount(int baseCount)
    {
        baseMaxTargetCount = Mathf.Max(1, baseCount);
    }


    public void SetMaxTargetCount(int maxCount)
    {
        SetBaseTargetCount(maxCount);
    }
}