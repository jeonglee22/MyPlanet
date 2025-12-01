using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    private Dictionary<int, Func<IMovement>> movementDict;

    private static MovementManager instance;
    public static MovementManager Instance => instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        movementDict = new Dictionary<int, Func<IMovement>>();

        movementDict.Add((int)MoveType.StraightDown, () => new StraightDownMovement());
        movementDict.Add((int)MoveType.Homing, () => new HomingMovement());
        movementDict.Add((int)MoveType.Chase, () => new ChaseMovement());
        movementDict.Add((int)MoveType.FollowParent, () => new FollowParentMovement());
        movementDict.Add((int)MoveType.DescendAndStopMovement, () => new DescendAndStopMovement());
        movementDict.Add((int)MoveType.Revolution, () => new RevolutionMovement());
        movementDict.Add((int)MoveType.Side, () => new SideMovement());
    }

    public IMovement GetMovement(int moveType)
    {
        if (movementDict.ContainsKey(moveType))
        {
            return movementDict[moveType]();
        }

        return null;
    }
}
