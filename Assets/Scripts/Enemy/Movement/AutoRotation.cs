using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    [SerializeField] private float selfRotateSpeed = 30f;

    void Update()
    {
        AutoRotate();
    }

    private void AutoRotate()
    {
        transform.Rotate(new Vector3(0f, selfRotateSpeed * Time.deltaTime, 0f), Space.Self);
    }
}
