using UnityEngine;

public class HitChangeSpeedChaseMovement : IMovement
{
    private bool isPatternLine = false;
    public bool IsPatternLine => isPatternLine;

    private bool isDirectionSet = false;
    private bool hasHit = false;

    private GameObject changeAsset;
    private GameObject changedAsset;

    public void Initialize(Enemy owner)
    {
        isDirectionSet = false;
        hasHit = false;

        if(changeAsset != null)
        {
            changedAsset.SetActive(true);
            changeAsset.SetActive(false);
        }

        if(changeAsset == null)
        {
            changedAsset = GameObject.FindWithTag(TagName.ChangedVisual);
            changeAsset = GameObject.FindWithTag(TagName.ChangeVisual);

            changeAsset?.SetActive(false);
        }
    }

    public Vector3 GetFinalDirection(Vector3 baseDirection, Transform ownerTransform, Transform target)
    {
        Vector3 chaseDirection = (target.position - ownerTransform.position).normalized;
        Vector3 lookDirection = new Vector3(chaseDirection.x, 0f, chaseDirection.z).normalized;
        if(lookDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            ownerTransform.rotation = Quaternion.RotateTowards(ownerTransform.rotation, toRotation, 720f * Time.deltaTime);
        }

        return chaseDirection;
    }

    public void OnHitedInMovement()
    {
        if(changeAsset != null)
        {
            changedAsset.SetActive(false);
            changeAsset.SetActive(true);
        }

        hasHit = true;
    }

    public float GetSpeedMultiplier() => hasHit ? 3f : 1f;

    public bool IsCompleted() => isDirectionSet;

    public void OnPatternLine()
    {
        
    }
}
