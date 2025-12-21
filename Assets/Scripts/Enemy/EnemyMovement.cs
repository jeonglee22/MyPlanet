using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Enemy owner;

    public float moveSpeed = 5f;
    private float initMoveSpeed;

    private IMovement currentMovement;
    public IMovement CurrentMovement { get => currentMovement; set => currentMovement = value; }

    protected Vector3 moveDirection;
    public Vector3 MoveDirection { get => moveDirection; set => moveDirection = value; }
    protected Transform target;

    private bool isDirectionSet = false;
    public bool CanMove { get; set; } = true;

    private int spawnPointIndex = -1;

    public bool isDebuff;
    private float debuffInterval = 2f;
    private float debuffTime;
    public float DebuffTime { get => debuffTime; set => debuffTime = value; }

    private Transform centerStone;
    private float orbitRadius = 1f;
    private bool hasReachedOribt = false;

    private void Start()
    {
        centerStone = GameObject.FindGameObjectWithTag(TagName.CenterStone).transform;
    }

    void OnEnable()
    {
        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag(TagName.Planet).transform;
        }

        owner = gameObject.GetComponent<Enemy>();

        isDebuff = false;
        isDirectionSet = false;
        debuffTime = 0f;
    }

    protected virtual void Update()
    {
        if(isDebuff && owner.EnemyType != 4)
            Debuff(Time.deltaTime);
        else
            ResetMovement();

        if (!CanMove || currentMovement == null)
        {
            return;
        }

        Vector3 baseDirection = CalculateDirection();

        Vector3 finalDirection = currentMovement.GetFinalDirection(baseDirection, transform, target);

        float speedMultiplier = currentMovement.GetSpeedMultiplier();

        transform.position += finalDirection * moveSpeed * speedMultiplier * Time.deltaTime;

        CheckOrbitReached();
    }

    private void Debuff(float time)
    {
        debuffTime += time;
        // Debug.Log("Enemy Moving : " + moveSpeed + " / Debuff Time: " + debuffTime);
        if(debuffTime > debuffInterval)
        {
            debuffTime = 0f;
            isDebuff = false;
            Initialize(moveSpeed, spawnPointIndex, owner.EnemyType, currentMovement);
        }
    }

    public virtual void Initialize(float speed, int spawnIndex, int enemyType, IMovement movement)
    {
        moveSpeed = speed;
        initMoveSpeed = speed;
        spawnPointIndex = spawnIndex;
        isDirectionSet = false;

        currentMovement = movement;
        currentMovement?.Initialize(owner);
    }

    private void SetTargetDirection()
    {
        var screenRect = SpawnManager.Instance.ScreenBounds;
        Vector3 randomPosition;

        if (spawnPointIndex == 1)
        {
            float quarterWidth = screenRect.width * 0.25f;
            randomPosition = new Vector3(Random.Range(screenRect.xMin, screenRect.xMin + quarterWidth), screenRect.yMin, 0f);
        }
        else if (spawnPointIndex == 3)
        {
            float quarterWidth = screenRect.width * 0.25f;
            randomPosition = new Vector3(Random.Range(screenRect.xMax - quarterWidth, screenRect.xMax), screenRect.yMin, 0f);
        }
        else
        {
            randomPosition = new Vector3(Random.Range(screenRect.xMin, screenRect.xMax), screenRect.yMin, 0f);
        }

        moveDirection = (randomPosition - transform.position).normalized;
    }

    private Vector3 CalculateDirection()
    {
        if (!isDirectionSet)
        {
            SetTargetDirection();
            isDirectionSet = true;
        }

        return moveDirection;
    }

    public void OnPatternLine()
    {
        currentMovement?.OnPatternLine();
    }

    public void ModifySpeed(float multiplier)
    {
        moveSpeed = initMoveSpeed * multiplier;
    }

    public void ResetSpeed()
    {
        moveSpeed = initMoveSpeed;
    }

    public void ResetMovement()
    {
        if(currentMovement != null && currentMovement.IsCompleted())
        {
            moveDirection = Vector3.zero;
            isDirectionSet = false;

            if(owner is ConstellationEnemy constellationEnemy)
            {
                constellationEnemy.DisableCollider();
            }
        }
    }

    private void CheckOrbitReached()
    {
        if (hasReachedOribt)
        {
            return;
        }

        float distanceToCenter = Vector3.Distance(transform.position, centerStone.position);
        if (distanceToCenter <= orbitRadius)
        {
            hasReachedOribt = true;
            OnOrbitReached();
        }
    }

    public void OnOrbitReached()
    {
        owner?.OnOrbitReached();
    }
}
