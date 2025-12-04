using Unity.Android.Gradle.Manifest;
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
        if(isDebuff)
            Debuff(Time.deltaTime);
        else
            ResetMovement();

        if (!CanMove || currentMovement == null)
        {
            return;
        }

        Vector3 baseDirection = CalculateDirection();

        Vector3 finalDirection = currentMovement.GetFinalDirection(baseDirection, transform, target);

        transform.position += finalDirection * moveSpeed * Time.deltaTime;
    }

    private void Debuff(float time)
    {
        debuffTime += time;
        // Debug.Log("Enemy Moving : " + moveSpeed + " / Debuff Time: " + debuffTime);
        if(debuffTime > debuffInterval)
        {
            debuffTime = 0f;
            isDebuff = false;
            Initialize(initMoveSpeed, spawnPointIndex, currentMovement);
        }
    }

    public virtual void Initialize(float speed, int spawnIndex, IMovement movement)
    {
        moveSpeed = speed;
        initMoveSpeed = speed;
        spawnPointIndex = spawnIndex;
        isDirectionSet = false;

        currentMovement = movement;
        currentMovement?.Initialize(owner.Data.EnemyType);
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

    private void ResetMovement()
    {
        if(currentMovement != null && currentMovement.IsCompleted())
        {
            moveDirection = Vector3.zero;
            isDirectionSet = false;
            currentMovement = null;
        }
    }
}
