using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected float moveSpeed = 5f;
    protected Vector3 moveDirection;

    protected Transform player;

    protected bool isDirectionSet = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Planet").transform;
    }

    protected virtual void Update()
    {
        Move();
    }

    protected abstract void Move();

    public virtual void Initialize(float speed, Vector3 targetDirection)
    {
        moveSpeed = speed;
        moveDirection = targetDirection.normalized;
    }

    protected abstract void SetTargetDirection();
}
