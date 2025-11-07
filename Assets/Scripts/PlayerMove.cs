using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float autoRotateSpeed = 1.0f;

    private bool autoMoving;

    private Vector2 moveVector;

    private bool clockRotate;
    private float currentAngle;

    void Start()
    {
        autoMoving = true;
        clockRotate = true;
        currentAngle = -90f;
        var startPos = CalculatePosOnCircle(currentAngle);

        transform.position = transform.parent.position + startPos * 1.5f;
    }

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
        autoMoving = moveVector.magnitude <= 0.2f;
    }

    private void Update()
    {
        if (autoMoving)
            AutoMove();
        else
            Move();
    }

    private void AutoMove()
    {
        Debug.Log(currentAngle);
        currentAngle += autoRotateSpeed * Time.deltaTime * (clockRotate ? -1f : 1f);

        var newPos = CalculatePosOnCircle(currentAngle);
        transform.position = transform.parent.position + newPos * 1.5f;
    }

    private void Move()
    {
        var magnitude = moveVector.magnitude;

        if (magnitude <= 0.2f) return;

        Vector2 unitVector = moveVector.normalized;
        Vector3 offSet = new Vector3(unitVector.x, unitVector.y, 0);
        transform.position = transform.parent.position + offSet * 1.5f;

        currentAngle = Vector3.Angle(Vector3.right, offSet);
        if (offSet.y < 0)
            currentAngle *= -1f;
        
        Debug.Log(currentAngle);
    }
    
    private Vector3 CalculatePosOnCircle(float angle)
    {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
    }
}
