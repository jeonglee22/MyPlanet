using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirePillarLazerPattern : SkillBasedLazerPattern
{
    protected override Transform GetLazerTarget()
    {
        return target;
    }
}
