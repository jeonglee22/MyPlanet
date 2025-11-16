using System;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public GameObject joyStick;

    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float autoRotateSpeed = 1.0f;
    [SerializeField] private float xAxisLimit = 3f;
    [SerializeField] private float yAxisLimit = 3f;
    [SerializeField] private bool isMoveEllipse = false;

    private float eccentricity;

    private float selfRotateSpeed = 30f;

    private float posOffset = 1.5f;

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
        if(isMoveEllipse)
        {
            var radius = xAxisLimit / (1 + eccentricity * Mathf.Cos(currentAngle * Mathf.Deg2Rad));
            startPos = new Vector3(radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad), radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0);
            transform.position = transform.parent.position + startPos + new Vector3(eccentricity * xAxisLimit, 0, 0);
        }
        else
        {
            startPos = CalculatePosOnCircle(currentAngle);
            transform.position = transform.parent.position + startPos * posOffset;
        }

        // var startPos = CalculatePosOnCircle(currentAngle);

        // transform.position = transform.parent.position + startPos * posOffset;
    }

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
        autoMoving = false;
    }

    private void Update()
    {
        if (!joyStick.activeSelf)
            autoMoving = true;

        if (autoMoving)
            AutoMove();
        else
        {
            if(isMoveEllipse)
            {
                eccentricity = Mathf.Sqrt(1 - (yAxisLimit * yAxisLimit) / (xAxisLimit * xAxisLimit));
                EllipseMove();
            }
            else
                Move();
        }
            

        AutoRotate();
    }

    private void AutoRotate()
    {
        transform.Rotate(new Vector3(0f, 0f, selfRotateSpeed * Time.deltaTime), Space.World);
    }

    private void AutoMove()
    {
        currentAngle += autoRotateSpeed * Time.deltaTime * (clockRotate ? -1f : 1f);
        if(currentAngle >= 180f)
            currentAngle -= 360f;
        else if(currentAngle < -180f)
            currentAngle += 360f;

        var newPos = Vector3.zero;
        if(isMoveEllipse)
        {
            var radius = xAxisLimit / (1 + eccentricity * Mathf.Cos(currentAngle * Mathf.Deg2Rad));
            newPos = new Vector3(radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad), radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0);
            transform.position = transform.parent.position + newPos + new Vector3(eccentricity * xAxisLimit, 0, 0);
        }
        else
        {
            newPos = CalculatePosOnCircle(currentAngle);
            transform.position = transform.parent.position + newPos * posOffset;
        }
        
    }

    private void EllipseMove()
    {
        var magnitude = moveVector.magnitude;

        if (magnitude <= 0.2f) return;

        Vector2 unitVector = moveVector.normalized;
        Vector3 contolPos = new Vector3(unitVector.x, unitVector.y, 0);

        float diffAngle = Vector3.Angle(contolPos, CalculatePosOnCircle(currentAngle));
        if(diffAngle > 178f)
        {
            AutoMove();
            return;
        }   

        var joystickAngle = Mathf.Atan2(unitVector.y, unitVector.x) * Mathf.Rad2Deg;
        var angleDelta = Mathf.DeltaAngle(currentAngle, joystickAngle);
        var rotateAngle = moveSpeed * magnitude * Time.deltaTime;

        if (Mathf.Abs(angleDelta) <= rotateAngle)
        {
            currentAngle = joystickAngle;
        }
        else
        {
            currentAngle += Mathf.Sign(angleDelta) * rotateAngle;
        }

        currentAngle = Mathf.Repeat(currentAngle + 180f, 360f) - 180f;

        var radius = xAxisLimit * ( 1 - eccentricity * eccentricity) / (1 + eccentricity * Mathf.Cos(currentAngle * Mathf.Deg2Rad));
        var newPos = new Vector3(radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad), radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0);
        transform.position = transform.parent.position + newPos;
    }

    private void Move()
    {
        var magnitude = moveVector.magnitude;

        if (magnitude <= 0.2f) return;

        Vector2 unitVector = moveVector.normalized;
        Vector3 contolPos = new Vector3(unitVector.x, unitVector.y, 0);

        float diffAngle = Vector3.Angle(contolPos, CalculatePosOnCircle(currentAngle));
        if(diffAngle > 178f)
        {
            AutoMove();
            return;
        }   

        var joystickAngle = Mathf.Atan2(unitVector.y, unitVector.x) * Mathf.Rad2Deg;
        var angleDelta = Mathf.DeltaAngle(currentAngle, joystickAngle);
        var rotateAngle = moveSpeed * magnitude * Time.deltaTime;

        if (Mathf.Abs(angleDelta) <= rotateAngle)
        {
            currentAngle = joystickAngle;
        }
        else
        {
            currentAngle += Mathf.Sign(angleDelta) * rotateAngle;
        }

        currentAngle = Mathf.Repeat(currentAngle + 180f, 360f) - 180f;
        Vector3 newPos = CalculatePosOnCircle(currentAngle);
        transform.position = transform.parent.position + newPos * posOffset;
    }
    
    private Vector3 CalculatePosOnCircle(float angle)
    {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
    }
}
