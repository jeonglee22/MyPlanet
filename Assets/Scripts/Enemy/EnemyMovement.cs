using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected float moveSpeed = 5f;
    protected Vector3 moveDirection;

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
}
