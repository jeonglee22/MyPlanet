using UnityEngine;

public class test : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered with " + other.gameObject.name);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Entered with " + collision.gameObject.name);
        Debug.Log(collision.contacts.Length);
    }

    void Update()
    {
        if(!TouchManager.Instance.IsTouching)
            return;
            
        Debug.Log("IsDragging : " + TouchManager.Instance.IsDragging.ToString());
        Debug.Log("IsHolding : " + TouchManager.Instance.IsHolding.ToString());
        Debug.Log("IsTouching : " + TouchManager.Instance.IsTouching.ToString());
    }
}
