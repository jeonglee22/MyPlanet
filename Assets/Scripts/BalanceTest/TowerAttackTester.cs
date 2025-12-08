using UnityEngine;

public class TowerAttackTester : MonoBehaviour
{
    public int towerAttackId = -1;
    private TowerAttack towerAttack;
    public float damage = 0f;
    public float attackSpeed = 0f;
    public float accuracy = 50f;
    public float grouping = 0f;
    public float projectileNum = 1f;
    public int projectile_ID = -1;
    public int randomAbilityGroup_ID = -1;

    private int currentTowerAttackId = -1;

    [SerializeField] private TowerDataSO[] towerDataSOs;

    void Start()
    {
    }

    private void Initialize()
    {
        towerAttackId = -1;
        damage = 0f;
        attackSpeed = 0f;
        accuracy = 50f;
        grouping = 0f;
        projectileNum = 1f;
        projectile_ID = -1;
        randomAbilityGroup_ID = -1;
        currentTowerAttackId = -1;
    }

    void FixedUpdate()
    {
        TestBalance();
    }

    protected virtual void TestBalance()
    {
        var planet = transform.parent.GetComponentInChildren<Planet>();
        if (planet == null)
            return;

        if (currentTowerAttackId == towerAttackId || towerAttackId == -1)
            return;

        planet.RemoveTowerAt(0);

        planet.SetAttackTower(towerDataSOs[towerAttackId],0);

        towerAttack = planet.GetAttackTowerToAmpTower(0);

        var projectileData = towerAttack.BaseProjectileData;
        if (projectileData == null)
            return;

        projectile_ID = projectileData.Projectile_ID;
        damage = projectileData.Attack;
        attackSpeed = towerAttack.AttackTowerData.fireRate;
        accuracy = towerAttack.AttackTowerData.Accuracy;
        grouping = towerAttack.AttackTowerData.grouping;
        projectileNum = towerAttack.AttackTowerData.projectileCount;
        randomAbilityGroup_ID = towerAttack.AttackTowerData.randomAbilityGroupId;

        currentTowerAttackId = towerAttackId;
    }
}
