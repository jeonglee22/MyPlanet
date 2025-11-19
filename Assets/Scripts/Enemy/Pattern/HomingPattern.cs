using UnityEngine;

public class HomingPattern : MovementPattern
{
    protected override void ChangeMovement()
    {
        if(movement != null)
        {
            Destroy(movement);
        }

        var homingMovement = owner.gameObject.AddComponent<HomingMovement>();
        homingMovement.Initialize(originalSpeed, Vector3.zero);
    }
}
