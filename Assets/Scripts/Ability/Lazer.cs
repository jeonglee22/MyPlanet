using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private BoxCollider boxCollider;
    private int pointCount = 40;
    private Transform target;
    private Transform tower;
    private Vector3 finalDirection;
    private Vector3 finalPoint;
    private float duration;
    private TowerAttack towerAttack;
    private bool isHoming;
    private List<int> abilities;
    private float durationTimer;
    private float attackDelayTimer;
    private float attackDelay = 0.5f;
    private List<Enemy> attackObject;
    private List<Enemy> removeObject;
    private float lazerLength = 10f;

    void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        attackObject = new List<Enemy>();
        removeObject = new List<Enemy>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        durationTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        durationTimer += Time.deltaTime;
        attackDelayTimer += Time.deltaTime;

        UpdateLazerPositions();
        if (attackDelayTimer >= attackDelay)
        {
            Debug.Log(attackObject.Count);
            foreach (var enemy in attackObject)
            {
                enemy.OnDeathEvent += () => {
                    removeObject.Add(enemy);
                };
                enemy.OnDamage(10f);
            }

            foreach (var rem in removeObject)
            {
                attackObject.Remove(rem);
            }
            attackDelayTimer = 0f;
        }

        CheckTarget();
        
        if (durationTimer >= duration)
        {
            Destroy(gameObject);
            towerAttack.IsStartLazer = false;
            durationTimer = 0f;
        }
    }

    private void CheckTarget()
    {
        if (target == null)
            return;

        var enemy = target.gameObject.GetComponent<Enemy>();
        if(enemy.IsDead)
        {
            target = null;
        }
    }

    private void UpdateLazerPositions()
    {
        lineRenderer.SetPosition(0, tower.position);
        Vector3 startPoint, endPoint, direction;
        if (target != null)
        {
            direction = (target.position - tower.position).normalized;

            startPoint = lineRenderer.GetPosition(0);
            endPoint = startPoint + direction * lazerLength;
            finalDirection = direction;
        }
        else
        {
            direction = finalDirection.normalized;

            startPoint = lineRenderer.GetPosition(0);
            endPoint = startPoint + direction * lazerLength;
        }
        

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t);

            if (isHoming)
                point += new Vector3(0, Mathf.Sin(t * Mathf.PI * 4) * 0.5f, 0); // Sine wave effect

            lineRenderer.SetPosition(i, point);
        }

        var angle = Vector3.Angle(Vector3.right, direction);
        if (direction.y < 0)
            angle = -angle;

        // var lerpAngle = Mathf.LerpAngle(transform.rotation.z, angle - 90f, 0.1f);
        // var clampAngle = Mathf.Abs(angle - 90f - lerpAngle) < 1f ? angle - 90f : lerpAngle;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        transform.position = tower.position;
    }

    public void SetLazer(Transform tower, Transform target, TowerAttack towerAttack, bool isHoming = false, float duration = 15f)
    {
        lineRenderer.positionCount = pointCount;
        Vector3 direction = (target.position - tower.position).normalized;

        this.tower = tower;
        this.target = target;
        finalPoint = tower.position + direction * lazerLength;
        this.duration = duration;
        this.towerAttack = towerAttack;
        this.isHoming = isHoming;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            Vector3 point = Vector3.Lerp(tower.position, finalPoint, t);
            if (isHoming)
                point += new Vector3(0, Mathf.Sin(t * Mathf.PI * 4) * 0.5f, 0); // Sine wave effect
            lineRenderer.SetPosition(i, point);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Lazer Hit");
        var damagable = other.gameObject.GetComponent<IDamagable>();
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (damagable != null && enemy != null)
        {
            attackObject.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy != null && attackObject.Contains(enemy))
        {
            attackObject.Remove(enemy);
        }
        
    }



    // public float CalculateTotalDamage(float enemyDef)
    // {
    //     var RatePanetration = Mathf.Clamp(this.RatePanetration, 0f, 100f);
    //     // Debug.Log(damage);
    //     var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - FixedPanetration;
    //     if(totalEnemyDef < 0)
    //     {
    //         totalEnemyDef = 0;
    //     }
    //     var totalDamage = damage * 100f / (100f + totalEnemyDef);
        
    //     return totalDamage;
    // }
}
