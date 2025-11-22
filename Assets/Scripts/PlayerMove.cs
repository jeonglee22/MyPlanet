using System;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerMove : MonoBehaviour
{
    public GameObject joyStick;

    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float autoRotateSpeed = 1.0f;
    [SerializeField] private float xAxisLimit = 1.5f;
    [SerializeField] private float yAxisLimit = 1.3f;
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
        SetPlanetPosition();

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
            Move();

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

        SetPlanetPosition();        
    }

    private void SetPlanetPosition()
    {
        if(isMoveEllipse)
        {
            var ellipsePos = GetPosOnEllipse();
            transform.position = transform.parent.position + ellipsePos;
        }
        else
        {
            var newPos = CalculatePosOnCircle(currentAngle);
            transform.position = transform.parent.position + newPos * posOffset;
        }
    }

    private Vector3 GetPosOnEllipse()
    {
        // var dir = CalculatePosOnCircle(currentAngle);
        // var ratio = dir.y / dir.x;
        // var xValue = Mathf.Sqrt(xAxisLimit * xAxisLimit * yAxisLimit * yAxisLimit / (yAxisLimit * yAxisLimit + xAxisLimit * xAxisLimit * ratio * ratio));
        // var yValue = ratio * xValue;
        // if (dir.x < 0) 
        // {
        //     xValue = -xValue;
        //     yValue = -yValue;
        // }

        var xValue = xAxisLimit * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        var yValue = yAxisLimit * Mathf.Sin(currentAngle * Mathf.Deg2Rad);

        return new Vector3(xValue, yValue, 0);
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
            clockRotate = angleDelta <= 0f;
        }

        currentAngle = Mathf.Repeat(currentAngle + 180f, 360f) - 180f;
        SetPlanetPosition();
    }
    
    private Vector3 CalculatePosOnCircle(float angle)
    {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
    }
}
