using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;

    private Vector2 moveVector;

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Move();
        
    }

    private void Move()
    {
        if (moveVector.magnitude <= 0.2f) return;

        Vector2 unitVector = moveVector.normalized;
        Vector3 offSet = new Vector3(unitVector.x, unitVector.y, 0);
        transform.position = transform.parent.position + offSet * 1.5f;
    }
}
