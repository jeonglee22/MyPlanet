using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirePillarLazer : SkillBasedLazer
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

    
    protected override async UniTask ChargePhaseWithTrackAsync(CancellationToken token)
    {
        float elapsedTime = 0f;
        fieldRenderer.enabled = true;

        if(laserParticle != null)
        {
            laserParticle.gameObject.SetActive(false);
        }

        while (elapsedTime < chargeTime)
        {
            token.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;

            Vector3 targetPosition = GetTargetPosition();
            direction = (targetPosition - startPoint).normalized;

            float distanceTarget = Vector3.Distance(startPoint, targetPosition);

            transform.position = startPoint;

            if(direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }

            fieldRenderer.transform.localScale = new Vector3(fieldWidth, distanceTarget, 1f);
            
            Vector3 fieldCenter = startPoint + direction * (distanceTarget * 0.5f);
            fieldRenderer.transform.position = fieldCenter;
            fieldRenderer.transform.rotation = transform.rotation;

            if(laserParticle != null)
            {
                laserParticle.transform.position = startPoint;
                laserParticle.transform.rotation = transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
            }

            await UniTask.Yield(token);
        }

        Vector3 finalTargetPosition = GetTargetPosition();
        direction = (finalTargetPosition - startPoint).normalized;

        Rect screenBounds = SpawnManager.Instance.ScreenBounds;
        float screenBottomY = screenBounds.yMin;
        float distanceBottom = startPoint.y - screenBottomY;
        laserLength = Mathf.Max(0.1f, distanceBottom);

        endPoint = startPoint + direction * laserLength;

        SetupFinalField();
    }
}
