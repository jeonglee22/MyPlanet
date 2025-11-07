using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public GameObject joyStick;

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
        autoMoving = false;
        // autoMoving = moveVector.magnitude <= 0.2f;
    }

    private void Update()
    {
        if (!joyStick.activeSelf)
            autoMoving = true;

        if (autoMoving)
            AutoMove();
        else
            Move();
    }

    private void AutoMove()
    {
        currentAngle += autoRotateSpeed * Time.deltaTime * (clockRotate ? -1f : 1f);

        var newPos = CalculatePosOnCircle(currentAngle);
        transform.position = transform.parent.position + newPos * 1.5f;
    }

    private void Move()
    {
        var magnitude = moveVector.magnitude;

        if (magnitude <= 0.2f) return;

        Vector2 unitVector = moveVector.normalized;
        Vector3 contolPos = new Vector3(unitVector.x, unitVector.y, 0);

        float diffAngle = Vector3.Angle(contolPos, CalculatePosOnCircle(currentAngle));
        bool isOnRight = Vector3.Cross(contolPos, CalculatePosOnCircle(currentAngle)).z > 0;

        Debug.Log(diffAngle);

        if (diffAngle >= 1f && diffAngle <= 170f)
        {
            clockRotate = isOnRight;

            currentAngle += moveSpeed * magnitude * Time.deltaTime * (clockRotate ? -1f : 1f);

            var newPos = CalculatePosOnCircle(currentAngle);
            transform.position = transform.parent.position + newPos * 1.5f;
        }
        else
        {
            
        }
        
        // transform.position = transform.parent.position + contolPos * 1.5f;

        // currentAngle = Vector3.Angle(Vector3.right, contolPos);
        // if (contolPos.y < 0)
        //     currentAngle *= -1f;
    }
    
    private Vector3 CalculatePosOnCircle(float angle)
    {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
    }
}
