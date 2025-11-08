using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData[] enemyDatas;

    public void SpawnEnemy(Vector3 position)
    {
        
    }
    
    private void AddMovementComponent(GameObject obj, EnemyData data)
    {
        EnemyMovement movement;

        switch (data.movementType)
        {
            case MovementType.Meteor:
                movement = obj.AddComponent<MeteorMovement>();
                break;
            case MovementType.Missile:
                break;
        }
    }
}
