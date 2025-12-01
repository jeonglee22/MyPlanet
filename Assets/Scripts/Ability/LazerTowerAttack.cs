using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class LazertowerAttack : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private BoxCollider boxCollider;
    private int pointCount = 2;
    private Transform target;
    private Transform tower;
    private Vector3 finalDirection;
    private float baseAngle;
    private Vector3 finalPoint;
    private Transform followingPoint;
    [SerializeField] private Transform tailTransform;
    public Transform TailTransform => tailTransform;
    private float duration;
    private TowerAttack towerAttack;
    private bool isHoming;
    private List<int> abilities;
    private Projectile projectile;
    private float damage;

    private event Action<GameObject> abilityAction;
    private event Action<GameObject> abilityRelease;
    private float durationTimer;
    private float attackDelayTimer;
    private float attackDelay = 0.1f;
    private List<Enemy> attackObject;
    private List<Enemy> removeObject;
    private float lazerLength = 10f;
    private bool isSplitSet = false;
    public bool IsSplitSet { get => isSplitSet; set => isSplitSet = value; }
    private LazertowerAttack splitBaseLazer = null;

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

        CheckTarget();
        
        if (durationTimer >= duration || tower == null)
        {
            Destroy(gameObject);
            towerAttack.IsStartLazer = false;
            durationTimer = 0f;
            projectile.gameObject.SetActive(true);
            projectile.IsFinish = true;
        }
    }

    void FixedUpdate()
    {
        attackDelayTimer += Time.deltaTime;

        if (tower == null)
            return;

        UpdateLazerPositions();
        if (attackDelayTimer >= attackDelay)
        {
            foreach (var enemy in attackObject)
            {
                abilityAction?.Invoke(enemy.gameObject);

                enemy.OnDamage(CalculateTotalDamage(enemy.Data.Defense));

                if (enemy.IsDead)
                {
                    removeObject.Add(enemy);
                }
            }

            foreach (var rem in removeObject)
            {
                attackObject.Remove(rem);
            }
            removeObject.Clear();

            attackDelayTimer = 0f;
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
        Vector3 startPoint, endPoint, direction;
    
        if (target != null)
        {
            direction = (target.position - tower.position).normalized;
            if (splitBaseLazer != null)
            {
                direction = Quaternion.Euler(0, 0, baseAngle) * splitBaseLazer.finalDirection.normalized;
            }

            startPoint = lineRenderer.GetPosition(0);
            endPoint = startPoint + direction * lazerLength;
            finalDirection = direction;

            if (isSplitSet)
            {
                endPoint = target.position;
                lazerLength = Vector3.Distance(tower.position, target.position);
            }
        }
        else
        {
            direction = finalDirection.normalized;
            if (splitBaseLazer != null)
            {
                direction = Quaternion.Euler(0, 0, baseAngle) * splitBaseLazer.finalDirection.normalized;
            }

            startPoint = tower.position;
            endPoint = startPoint + direction * lazerLength;
        }
        
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        tailTransform.position = endPoint;

        var angle = Vector3.Angle(Vector3.right, direction);
        if (direction.y < 0)
            angle = -angle;

        // var lerpAngle = Mathf.LerpAngle(transform.rotation.z, angle - 90f, 0.1f);
        // var clampAngle = Mathf.Abs(angle - 90f - lerpAngle) < 1f ? angle - 90f : lerpAngle;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        transform.position = tower.position;
    }

    public void SetLazer(Transform start, float angle, Transform target, Projectile projectile, TowerAttack towerAttack, float duration = 15f, bool isSplit = false, LazertowerAttack baseLazer = null)
    {
        lineRenderer.positionCount = pointCount;
        Vector3 newDirection = target == null ? Quaternion.Euler(0, 0, angle) * baseLazer.finalDirection : (target.position - start.position).normalized;
        finalDirection = newDirection;
        baseAngle = angle;
        splitBaseLazer = baseLazer;

        this.tower = start;
        this.target = target;
        finalPoint = start.position + newDirection * lazerLength;
        this.duration = duration;
        this.towerAttack = towerAttack;
        abilities = new List<int>(towerAttack.Abilities);
        Debug.Log(abilities.Count);
        if (isSplit)
        {
            abilities.RemoveAll(x => x == (int)AbilityId.Split);
        }
        this.projectile = projectile;
        damage = projectile.projectileData.Attack;

        foreach (var abilityId in abilities)
        {
            var ability = AbilityManager.GetAbility(abilityId);
            if (ability == null) continue;

            ability.ApplyAbility(projectile.gameObject);
            abilityAction += ability.ApplyAbility;
            abilityRelease += ability.RemoveAbility;
            ability.Setting(towerAttack.gameObject);
        }

        lineRenderer.SetPosition(0, tower.position);
        lineRenderer.SetPosition(1, finalPoint);
    }

    private void OnTriggerEnter(Collider other)
    {
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

    public float CalculateTotalDamage(float enemyDef)
    {
        var RatePanetration = Mathf.Clamp(projectile.projectileData.RatePenetration, 0f, 100f);
        // Debug.Log(damage);
        var totalEnemyDef = enemyDef * (1 - RatePanetration / 100f) - projectile.projectileData.FixedPenetration;
        if(totalEnemyDef < 0)
        {
            totalEnemyDef = 0;
        }
        var totalDamage = damage * 100f / (100f + totalEnemyDef);
        
        return totalDamage;
    }
}
