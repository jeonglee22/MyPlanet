using UnityEngine;

public class SplitDeathAbility : EnemyAbility
{
    [SerializeField] private int splitCount = 3;

    public override void Initialize(Enemy enemy, EnemyData enemyData)
    {
        base.Initialize(enemy, enemyData);
        owner.OnDeathEvent += Split;
    }

    private void OnDestroy()
    {
        owner.OnDeathEvent -= Split;
    }

    private void Split()
    {
        Vector3 basePos = transform.position;

        for(int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.down;

            //나중에 스폰 매니저에서 스폰하는걸로 바꾸기
            
        }
    }
}
