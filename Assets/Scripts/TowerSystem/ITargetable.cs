using System.Collections.Generic;
using UnityEngine;

public interface ITargetable // consider targeting object.
{
    Vector3 position { get; }
    bool isAlive { get; }
    float maxHp { get; }
    float atk { get; }
    float def { get; }
}

public interface ITargetPrioriy //consider targeting priority
{
    ITargetable SelectTarget(IEnumerable<ITargetable> targets);
}
