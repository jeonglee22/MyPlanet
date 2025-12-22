using UnityEngine;

public class KeepRotation : MonoBehaviour
{
    private Quaternion originalRotation;

    private void Awake()
    {
        originalRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = originalRotation;
    }
}
