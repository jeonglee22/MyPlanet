using System;
using UnityEngine;

public class BalanceTester : MonoBehaviour
{
    [Range(1, 100)]
    public float damage = 10f;
    public float attackSpeed = 1f;
    public float range = 5f;

    void Update()
    {
        if (TouchManager.Instance.IsTouching)
        {
            TestBalance();
        }
    }

    private void TestBalance()
    {
        transform.Rotate(Vector3.up, attackSpeed * 10f * Time.deltaTime);
        Debug.Log($"Testing Balance: Damage={damage}, AttackSpeed={attackSpeed}, Range={range}");
    }
}
