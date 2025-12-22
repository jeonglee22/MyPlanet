using UnityEngine;

public class EnemyDamageMove : MonoBehaviour
{
    private float movingUpSpeed = 2f;
    private float movingUpTime = 1f;
    private float currentTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * movingUpSpeed * Time.deltaTime;
        currentTime += Time.deltaTime;
        if(currentTime>=movingUpTime)
        {
            Destroy(gameObject);
        }
    }
}
