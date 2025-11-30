using UnityEngine;

public class test : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered with " + other.gameObject.name);

        Vector3 pos = other.ClosestPoint(transform.position);
        Debug.Log($"Trigger Hit Pos: {pos}");

        Debug.DrawLine(transform.position, pos, Color.red, 1f);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Entered with " + collision.gameObject.name);
        Debug.Log(collision.contacts.Length);
    }
}
