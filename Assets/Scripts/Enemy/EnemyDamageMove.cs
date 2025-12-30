using System;
using UnityEngine;

public class EnemyDamageMove : MonoBehaviour, IDisposable
{
    private float movingUpSpeed = 2.5f;
    private float movingUpTime = 0.5f;
    private float currentTime = 0f;

    public void Dispose()
    {
        currentTime = 0f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        currentTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * movingUpSpeed * Time.deltaTime;
        currentTime += Time.deltaTime;
        if(currentTime>=movingUpTime)
        {
            SpawnManager.Instance.ReturnDamagePopup(this);
        }
    }
}
