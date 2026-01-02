using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class LazertowerAttack : MonoBehaviour, System.IDisposable
{
    private LineRenderer lineRenderer;
    private BoxCollider boxCollider;
    private Vector3 initCenter;
    private Vector3 initSize;
    private int pointCount = 2;
    private Transform target;
    // private Vector3 hitPosition;
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
    private readonly List<Enemy> attackObject=new List<Enemy>(64);
    private readonly List<Enemy> removeObject=new List<Enemy>(16);
    private float lazerLength = 10f;
    private bool isSplitSet = false;
    public bool IsSplitSet { get => isSplitSet; set => isSplitSet = value; }
    private LazertowerAttack splitBaseLazer = null;
    private float lazerOffset;

    public float InitColliderWidth { get; private set; }
    public float InitLineRendererWidth { get; private set; }

    private Planet planet;
    private bool despawnRequested= false;

    private AudioSource laserAudioSource;

    void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        initCenter = boxCollider.center;
        initSize = boxCollider.size;
        initSize.y = 12f;
        boxCollider.size = initSize;

        InitColliderWidth = boxCollider.size.x;
        InitLineRendererWidth = lineRenderer != null ? lineRenderer.endWidth : 0f;

        //attackObject = new List<Enemy>();
        //removeObject = new List<Enemy>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        durationTimer = 0f;
        planet = GameObject.FindWithTag(TagName.Planet).GetComponent<Planet>();

        if(laserAudioSource != null)
        {
            StopLaserSound();
        }

        if(SoundManager.Instance != null && SoundManager.Instance.IsInitialized)
        {
            laserAudioSource = SoundManager.Instance.PlayLaserShotLoop(transform.position);
        }
    }

    private void OnEnable()
    {
        despawnRequested = false;
        durationTimer = 0f;
        attackDelayTimer= 0f;

        attackObject.Clear();
        removeObject.Clear();
    }
    private void OnDestroy()
    {
        StopLaserSound();
    }

    private void OnDisable()
    {
        StopLaserSound();
    }
    
    

    // Update is called once per frame
    void Update()
    {
        durationTimer += Time.deltaTime;

        if (CameraManager.Instance.IsZoomedOut)
        {
            lazerLength = 30f;
            boxCollider.center = new Vector3(0f, 15f, 0f);
            var newSize = initSize;
            newSize.y = lazerLength;
            boxCollider.size = newSize;
        }
        else
        {
            lazerLength = 12f;
            boxCollider.center = new Vector3(0f, 6f, 0f);
        }

        CheckTarget();

        //if(laserAudioSource != null)
        //{
        //    laserAudioSource.transform.position = transform.position;
        //}

        if (durationTimer >= duration || tower == null)
        {
            StopLaserSound();
            Despawn();
            //    Destroy(gameObject);
            //    towerAttack.IsStartLazer = false;
            //    durationTimer = 0f;
            //    projectile.gameObject.SetActive(true);
            //    projectile?.ReturnProjectileToPool();
            //}
        }
    }

        void FixedUpdate()
        {
            attackDelayTimer += Time.deltaTime;

            if (tower == null) return;

            UpdateLazerPositions();
        //if (attackDelayTimer >= attackDelay)
        //{
        //    foreach (var enemy in attackObject)
        //    {
        //        abilityAction?.Invoke(enemy.gameObject);

        //        var damage = CalculateTotalDamage(enemy.Data.Defense);
        //        enemy.OnDamage(damage);
        //        towerAttack.AdddamageDealt(damage);

        //        if (enemy.IsDead)
        //        {
        //            removeObject.Add(enemy);
        //        }
        //    }

        //    foreach (var rem in removeObject)
        //    {
        //        attackObject.Remove(rem);
        //    }
        //    removeObject.Clear();

        //    attackDelayTimer = 0f;
        //}
        if (attackDelayTimer >= attackDelay)
        {
            for (int i = 0; i < attackObject.Count; i++)
            {
                var enemy = attackObject[i];
                if (enemy == null) { removeObject.Add(enemy); continue; }

                abilityAction?.Invoke(enemy.gameObject);

                var d = CalculateTotalDamage(enemy.Data.Defense);
                enemy.OnDamage(d);
                towerAttack?.AdddamageDealt(d);

                if (enemy.IsDead)
                    removeObject.Add(enemy);
            }
            if (removeObject.Count > 0)
            {
                for (int i = 0; i < removeObject.Count; i++)
                    attackObject.Remove(removeObject[i]);
                removeObject.Clear();
            }
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
            // if (abilities.Contains((int)AbilityId.Split))
            // {
            //     if (target != null)
            //     {
            //         Ray ray = new Ray(tower.position, direction);
            //         if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Enemy")))
            //         {
            //             target = hitInfo.transform;
            //             finalPoint = hitInfo.point;
            //         }
            //     }
            // }

            if (splitBaseLazer != null)
            {
                direction = Quaternion.Euler(0, 0, baseAngle) * splitBaseLazer.finalDirection.normalized;
            }

            startPoint = tower.position;
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
            // if (abilities.Contains((int)AbilityId.Split))
            // {
            //     if (target != null)
            //     {
            //         Ray ray = new Ray(tower.position, direction);
            //         if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Enemy")))
            //         {
            //             target = hitInfo.transform;
            //             finalPoint = hitInfo.point;
            //         }
            //     }
            // }

            if (splitBaseLazer != null)
            {
                direction = Quaternion.Euler(0, 0, baseAngle) * splitBaseLazer.finalDirection.normalized;
            }

            startPoint = tower.position;
            endPoint = startPoint + direction * lazerLength;
        }
        
        var verticalDirection = Quaternion.Euler(0, 0, 90f) * finalDirection;
        boxCollider.center = initCenter + lazerOffset * verticalDirection.normalized;
        
        lineRenderer.SetPosition(0, startPoint + lazerOffset * verticalDirection.normalized);
        lineRenderer.SetPosition(1, endPoint + lazerOffset * verticalDirection.normalized);
        tailTransform.position = endPoint;

        var angle = Vector3.Angle(Vector3.right, direction);
        if (direction.y < 0)
            angle = -angle;

        // var lerpAngle = Mathf.LerpAngle(transform.rotation.z, angle - 90f, 0.1f);
        // var clampAngle = Mathf.Abs(angle - 90f - lerpAngle) < 1f ? angle - 90f : lerpAngle;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        transform.position = tower.position;
    }

    public void SetLazer(
        Transform start, 
        float angle, 
        Transform target, 
        Projectile projectile, 
        TowerAttack towerAttack, 
        float duration = 15f, 
        bool isSplit = false, 
        LazertowerAttack baseLazer = null)
    {
        Debug.Log($"[LASER][SetLazer] self={name} start={start?.name} target={(target ? target.name : "null")} duration={duration} isSplit={isSplit} base={(baseLazer ? baseLazer.name : "null")}");

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
        // Debug.Log(abilities.Count);
        if (isSplit)
        {
            abilities.RemoveAll(x => x == (int)AbilityId.Split);
        }
        this.projectile = projectile;
        damage = projectile.projectileData.Attack;

        InitColliderWidth = GetComponent<BoxCollider>().size.x;
        InitLineRendererWidth = lineRenderer.endWidth;

        ApplyHitSizeScaleFromProjectile();

        foreach (var abilityId in abilities)
        {
            var ability = AbilityManager.GetAbility(abilityId);
            if (ability == null) continue;

            ability.ApplyAbility(projectile.gameObject);
            ability.ApplyAbility(gameObject);
            abilityAction += ability.ApplyAbility;
            abilityRelease += ability.RemoveAbility;
            ability.Setting(towerAttack.gameObject);
        }

        lineRenderer.SetPosition(0, tower.position);
        lineRenderer.SetPosition(1, finalPoint);
    }

    public void SetLazerPositionOffset(float offset)
    {
        lazerOffset = offset * 4f;
        var verticalDirection = Quaternion.Euler(0, 0, 90f) * finalDirection;
        boxCollider.center = initCenter + lazerOffset * verticalDirection.normalized;
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

    private void ApplyHitSizeScaleFromProjectile() 
    {
        if (projectile == null) return;
        if (lineRenderer == null && boxCollider == null) return;
        var baseData = projectile.BaseData != null
            ? projectile.BaseData
            : projectile.projectileData;

        if (baseData == null) return;
        float baseSize = baseData.CollisionSize;
        float finalSize = projectile.projectileData.CollisionSize;

        if (baseSize <= 0f) return;
        float factor = finalSize / baseSize; 

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = InitLineRendererWidth * factor;
            lineRenderer.endWidth = InitLineRendererWidth * factor;
        }

        if (boxCollider != null)
        {
            var size = boxCollider.size;
            size.x = InitColliderWidth * factor;
            size.z = InitColliderWidth * factor;
            boxCollider.size = size;
        }
    }
    public void Despawn()
    {
        Debug.Log($"[LASER][END] self={name} durationTimer={durationTimer:0.00}/{duration:0.00} towerNull={(tower == null)} targetNull={(target == null)}");

        if (despawnRequested) return;
        despawnRequested = true;

        if (projectile != null)
        {
            projectile.gameObject.SetActive(true);
            projectile.ReturnProjectileToPool();
            projectile = null;
        }

        if (towerAttack != null)
            towerAttack.IsStartLazer = false;

        if (LaserPoolManager.Instance != null)
            LaserPoolManager.Instance.ReturnLaser(this);
        else
            gameObject.SetActive(false);
    }

    public void Dispose()
    {
        abilityAction = null;
        abilityRelease = null;

        tower = null;
        target = null;
        towerAttack = null;
        splitBaseLazer = null;

        isSplitSet = false;
        lazerOffset = 0f;
        baseAngle = 0f;

        duration = 0f;
        damage = 0f;

        abilities?.Clear();
        abilities = null;

        attackObject.Clear();
        removeObject.Clear();

        durationTimer = 0f;
        attackDelayTimer = 0f;

        if (boxCollider != null)
        {
            boxCollider.center = initCenter;
            boxCollider.size = initSize;
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = pointCount;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
            lineRenderer.startWidth = InitLineRendererWidth;
            lineRenderer.endWidth = InitLineRendererWidth;
        }

        despawnRequested = false;
    }

    private void StopLaserSound()
    {
        if(laserAudioSource != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopLaserShotLoop(laserAudioSource);
            laserAudioSource = null;
        }
    }
}
