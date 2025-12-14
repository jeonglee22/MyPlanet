using UnityEngine;

public class TwoPhaseHomingMovement : IMovement
{
    private enum MovementPhase
    {
        Diagonal,
        Homing,
    }

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;
    private bool isDirectionSet = false;
    private Vector3 currentMoveDirection;
    private bool isDirectionCalculated = false;

    private int enemyType;

    private MovementPhase currentPhase = MovementPhase.Diagonal;
    private float traveledDistance = 0f;
    private float phaseTransitionDistance = 2f;
    private Vector3 lastPosition;

    public void Initialize(int enemyType)
    {
        isDirectionSet = false;
        this.enemyType = enemyType;

        currentPhase = MovementPhase.Diagonal;
        isDirectionCalculated = false;
        traveledDistance = 0f;

        if(enemyType <= 1)
        {
            isPatternLine = true;
        }
        else
        {
            isPatternLine = false;
        }
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if (!isPatternLine)
        {
            return baseDirection;
        }

        if(currentPhase == MovementPhase.Diagonal)
        {
            if (!isDirectionCalculated)
            {
                SetDiagonalDirection(ownerTransform, target);
                lastPosition = ownerTransform.position;
                isDirectionCalculated = true;
            }

            float movedThisFrame = Vector3.Distance(ownerTransform.position, lastPosition);
            traveledDistance += movedThisFrame;
            lastPosition = ownerTransform.position;

            if(traveledDistance >= phaseTransitionDistance)
            {
                currentPhase = MovementPhase.Homing;
                isDirectionCalculated = false;
            }

            return currentMoveDirection;
        }

        if(!isDirectionCalculated && target != null)
        {
            currentMoveDirection = (target.position - ownerTransform.position).normalized;
            ownerTransform.LookAt(ownerTransform.position + currentMoveDirection);
            isDirectionCalculated = true;
        }

        return currentMoveDirection;
    }

    public void OnPatternLine()
    {
        if(enemyType <= 1)
        {
            return;
        }
        isPatternLine = true;
    }

    public bool IsCompleted() => isDirectionSet;

    public float GetSpeedMultiplier() => 1f;

    private void SetDiagonalDirection(Transform ownerTransform, Transform target)
    {
        if(target == null)
        {
            currentMoveDirection = new Vector3(1, 1, 0).normalized;
            return;
        }

        if(ownerTransform.position.x < target.position.x)
        {
            currentMoveDirection = new Vector3(1, 1, 0).normalized;
        }
        else
        {
            currentMoveDirection = new Vector3(-1, 1, 0).normalized;
        }

        ownerTransform.LookAt(ownerTransform.position + currentMoveDirection);
    }
}
