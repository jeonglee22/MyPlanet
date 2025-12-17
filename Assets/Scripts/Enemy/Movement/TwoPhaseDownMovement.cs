using UnityEngine;

public class TwoPhaseDownMovement : IMovement
{
    private enum MovementPhase
    {
        Horizontal,
        Wait,
        Down,
    }

    private Enemy owner;

    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;
    private bool isDirectionSet = false;
    private Vector3 currentMoveDirection;
    private bool isDirectionCalculated = false;

    private int enemyType;

    private MovementPhase currentPhase = MovementPhase.Horizontal;
    private float traveledDistance = 0f;
    private float phaseTransitionDistance = 2f;
    private Vector3 lastPosition;

    private float waitTime = 0f;
    private float waitDuration = 1f;

    public void Initialize(Enemy owner)
    {
        this.owner = owner;

        isDirectionSet = false;
        enemyType = owner.EnemyType;

        currentPhase = MovementPhase.Horizontal;
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

        waitTime = 0f;
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        if (!isPatternLine)
        {
            return baseDirection;
        }

        if(currentPhase == MovementPhase.Horizontal)
        {
            if (!isDirectionCalculated)
            {
                SetHorizontalDirection(ownerTransform);
                lastPosition = ownerTransform.position;
                isDirectionCalculated = true;
            }

            float movedThisFrame = Vector3.Distance(ownerTransform.position, lastPosition);
            traveledDistance += movedThisFrame;
            lastPosition = ownerTransform.position;

            if(traveledDistance >= phaseTransitionDistance)
            {
                currentPhase = MovementPhase.Wait;
                isDirectionCalculated = false;
            }

            ownerTransform.LookAt(ownerTransform.position + currentMoveDirection);

            return currentMoveDirection;
        }

        if(currentPhase == MovementPhase.Wait)
        {
            waitTime += Time.deltaTime;
            if(waitTime >= waitDuration)
            {
                currentPhase = MovementPhase.Down;
                isDirectionCalculated = false;
            }

            return Vector3.zero;
        }

        if(!isDirectionCalculated && target != null)
        {
            currentMoveDirection = Vector3.down;
            isDirectionCalculated = true;
        }

        ownerTransform.LookAt(ownerTransform.position + currentMoveDirection);

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

    private void SetHorizontalDirection(Transform ownerTransform)
    {
        var parent = owner.ParentEnemy != null ? owner.ParentEnemy.transform : owner.Spawner.transform;

        if(ownerTransform.position.x > parent.position.x)
        {
            currentMoveDirection = new Vector3(1, -1, 0).normalized;
        }
        else
        {
            currentMoveDirection = new Vector3(-1, -1, 0).normalized;
        }

        ownerTransform.LookAt(ownerTransform.position + currentMoveDirection);
    }
}
