using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SkillBasedParticleLazer : SkillBasedLazer
{
    protected override void Setup()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.useWorldSpace = true;

        Color transparentColor = new Color(1f, 1f, 1f, 0f);
        lineRenderer.startColor = transparentColor;
        lineRenderer.endColor = transparentColor;
    }
    
    protected override async UniTask ChargePhaseWithTrackAsync(CancellationToken ct)
    {
        base.ChargePhaseWithTrackAsync(ct).Forget();

        SetupFinalField();
    }
}
