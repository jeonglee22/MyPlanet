using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    private Vector3 moveDirection;

    protected virtual void Update()
    {
        Move();
    }

    protected abstract void Move();
}
