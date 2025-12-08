using UnityEngine;

public class TowerAttackTester : MonoBehaviour
{
    public int towerAttackId;
    private TowerAttack towerAttack;
    public float damage;
    public float attackSpeed;
    public float attackRange;
    public float accuracy;
    public float grouping;
    public float projectileNum;
    public int projectile_ID;
    public int randomAbilityGroup_ID;

    [SerializeField] private TowerDataSO[] towerDataSOs;

    void FixedUpdate()
    {
        TestBalance();
    }

    protected virtual void TestBalance()
    {
        var planet = transform.parent.GetComponentInChildren<Planet>();
        if (planet == null)
            return;

        planet?.SetAttackTower(towerDataSOs[towerAttackId],0);
        if (towerAttack == null)
            towerAttack = planet.GetAttackTowerToAmpTower(0);

        // towerAttack.Damage = damage;
        // towerAttack.AttackSpeed = attackSpeed;
        // towerAttack.AttackRange = attackRange;
        // towerAttack.Accuracy = accuracy;
        // towerAttack.grouping = grouping;
        // towerAttack.ProjectileNum = projectileNum;
        // towerAttack.Projectile_ID = projectile_ID;
        // towerAttack.RandomAbilityGroup_ID = randomAbilityGroup_ID;

        transform.Rotate(Vector3.up, attackSpeed * 10f * Time.deltaTime);
        Debug.Log($"Testing Tower Attack:");
    }
}
