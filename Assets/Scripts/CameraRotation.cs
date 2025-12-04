using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;

    private Material skyBox;

    private void Start()
    {
        skyBox = RenderSettings.skybox;
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
