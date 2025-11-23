using UnityEngine;

public class test : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered with " + other.gameObject.name);
    }
}
