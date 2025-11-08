using UnityEngine;

public abstract class EnemyAbility : MonoBehaviour
{
    protected Enemy owner;
    protected EnemyData data;

    public virtual void Initialize(Enemy enemy, EnemyData enemyData)
    {
        owner = enemy;
        data = enemyData;
    }
}
