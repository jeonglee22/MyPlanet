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

    public bool CanMove { get; set; } = true;

    void OnEnable()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag(TagName.Planet).transform;
        }

        isDebuff = false;
    }

    protected virtual void Update()
    {
        if(isDebuff)
        {
            Debuff();
        }

        if (!CanMove)
        {
            return;
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

    protected virtual void Move()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag(TagName.Planet).transform;
        }
    }

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
