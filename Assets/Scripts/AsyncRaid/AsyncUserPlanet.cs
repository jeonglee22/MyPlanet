using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AsyncUserPlanet : LivingEntity
{
    [SerializeField] HighestHpPrioritySO highestHpPrioritySO;

    private float livingTime;
    private float elapsedTime = 0f;
    private float attack = 0f;
    private float dieDps;
    private float attackDps;
    private float attackTimer = 0f;

    private TowerAttack tower;
    private List<int> abilities;

    private UserPlanetData planetData;
    private string blurNickname;
    public string BlurNickname => blurNickname;
    private AsyncPlanetData asyncPlanetData;
    private TowerDataSO towerDataSO;
    private float currentDamage;

    private void Awake()
    {
        // livingTime = Random.Range(5f, 10f);
        livingTime = Random.Range(20f, 40f);
        abilities = new List<int>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void Update()
    {
        // elapsedTime += Time.deltaTime;
        OnDamage(dieDps * Time.deltaTime);
        
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f)
        {
            Variables.LastBossEnemy?.OnDamage(attackDps);
            currentDamage += attackDps;
            Debug.Log("AsyncUserPlanet Deal Damage : " + currentDamage);
            attackTimer = 0f;
        }
    }

    private void OnDisable()
    {
        Debug.Log("Damage to Boss : " + attack);
    }

    public void InitializePlanet(UserPlanetData data, float damage, AsyncPlanetData asyncData, TowerDataSO towerData)
    {
        if (data == null)
        {
            data = new UserPlanetData("Unknown", 0);
        }

        planetData = data;
        attack = damage;
        attackDps = attack / livingTime;
        dieDps = Health / livingTime;
        tower = GetComponent<TowerAttack>();
        tower.IsOtherUserTower = true;

        SetBlurNickname(planetData.nickName);
        asyncPlanetData = asyncData;
        var towerId = asyncPlanetData.AttackTower_Id;
        towerDataSO = ScriptableObject.Instantiate(towerData);
        towerDataSO.targetPriority = highestHpPrioritySO;

        tower.SetTowerData(towerDataSO);
        // var projectileData = tower.BaseProjectileData;
        tower.DamageBuffMul = 0f;

        var targetingSystem = tower.GetComponent<TowerTargetingSystem>();
        targetingSystem.SetTowerData(towerDataSO);

        SetupAbilities();
        foreach (var abilityId in abilities)
        {
            var ability = AbilityManager.GetAbility(abilityId);
            tower.AddAbility(abilityId);
            ability?.ApplyAbility(tower.gameObject);

        }
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
    }

    public override void Die()
    {
        base.Die();
    }

    private void SetBlurNickname(string nickname)
    {
        if (nickname.Length <= 2)
        {
            blurNickname = $"{nickname[0]}*";
            return;
        }
        if (nickname.Length == 3)
        {
            blurNickname = $"{nickname[0]}*{nickname[2]}";
            return;
        }

        var sb = new StringBuilder();
        sb.Append(nickname.Substring(0, 2));
        for (int i = 2; i < nickname.Length - 1; i++)
        {
            sb.Append("*");
        }
        sb.Append(nickname[nickname.Length - 1]);

        Debug.Log(sb.ToString());
        blurNickname = sb.ToString();
    }

    private void SetupAbilities()
    {
        if (asyncPlanetData == null)
            return;

        if (abilities != null)
            abilities.Clear();
        abilities = new List<int>();

        var id1 = DataTableManager.RandomAbilityTable.GetAbilityIdFromEffectId(asyncPlanetData.Effect_Id_1);
        var id2 = DataTableManager.RandomAbilityTable.GetAbilityIdFromEffectId(asyncPlanetData.Effect_Id_2);
        var id3 = DataTableManager.RandomAbilityTable.GetAbilityIdFromEffectId(asyncPlanetData.Effect_Id_3);

        if (id1 != -1)
            abilities.Add(id1);
        if (id2 != -1)
            abilities.Add(id2);
        if (id3 != -1)
            abilities.Add(id3);
    }
}
