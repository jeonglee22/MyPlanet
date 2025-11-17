using System;
using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    protected Vector3 moveDirection;
    public Vector3 MoveDirection { get => moveDirection; set => moveDirection = value; }

    protected Transform player;

    protected bool isDirectionSet = false;

    public bool isDebuff;
    private float debuffInterval = 2f;
    private float debuffTime;

    private float initMoveSpeed;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Planet").transform;
    }

    void OnEnable()
    {
        isDebuff = false;
    }

    protected virtual void Update()
    {
        if(isDebuff)
        {
            Debuff();
        }

        Move();
    }

    private void Debuff()
    {
        debuffTime += Time.deltaTime;
        if(debuffTime > debuffInterval)
        {
            debuffTime = 0f;
            isDebuff = false;
            Initialize(initMoveSpeed, moveDirection);
        }
    }

    protected abstract void Move();

    public virtual void Initialize(float speed, Vector3 targetDirection)
    {
        moveSpeed = speed;
        initMoveSpeed = moveSpeed;
        moveDirection = targetDirection.normalized;
    }

    protected abstract void SetTargetDirection();

    public virtual void SetDirection(Vector3 direction)
    {
        
    }
}
